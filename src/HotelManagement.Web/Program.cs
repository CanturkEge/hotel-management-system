using System.Globalization;
using System.Threading.RateLimiting;
using HotelManagement.Application.DTOs;
using HotelManagement.Application.Interfaces;
using HotelManagement.Application.Services;
using HotelManagement.Infrastructure;
using HotelManagement.Infrastructure.Data;
using HotelManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var setup=args.Contains("--setup");
var builder=WebApplication.CreateBuilder(args.Where(x=>x!="--setup").ToArray());
builder.Services.AddControllersWithViews(options=>options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDbContext<HotelDbContext>(options=>options.UseNpgsql(
    builder.Configuration.GetConnectionString("HotelDatabase") ?? "Host=localhost;Database=hotel_not_configured;Username=postgres",
    postgres=>postgres.MigrationsHistoryTable("__EFMigrationsHistory","hotel")));
builder.Services.AddIdentity<AppUser,IdentityRole<Guid>>(options=> {
    options.User.RequireUniqueEmail=true;
    options.Password.RequiredLength=12;
    options.Password.RequireDigit=true;
    options.Password.RequireLowercase=true;
    options.Password.RequireUppercase=true;
    options.Password.RequireNonAlphanumeric=true;
    options.Lockout.MaxFailedAccessAttempts=5;
    options.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromMinutes(15);
}).AddEntityFrameworkStores<HotelDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options=> {
    options.LoginPath="/Account/Login"; options.AccessDeniedPath="/Account/Denied";
    options.Cookie.Name="HotelManagement.Session"; options.Cookie.HttpOnly=true;
    options.Cookie.SameSite=SameSiteMode.Lax;
    options.Cookie.SecurePolicy=builder.Environment.IsDevelopment()?CookieSecurePolicy.SameAsRequest:CookieSecurePolicy.Always;
    options.ExpireTimeSpan=TimeSpan.FromHours(4); options.SlidingExpiration=true;
});
builder.Services.Configure<SecurityStampValidatorOptions>(options=>options.ValidationInterval=TimeSpan.Zero);
builder.Services.Configure<ForwardedHeadersOptions>(options=> {
    options.ForwardedHeaders=ForwardedHeaders.XForwardedFor|ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddScoped(typeof(IRepository<>),typeof(EfRepository<>));
builder.Services.AddScoped<IUnitOfWork,UnitOfWork>();
builder.Services.AddScoped<IHotelService,HotelService>();
builder.Services.AddScoped<IAccountService,AccountService>();
builder.Services.AddSingleton<IHotelClock,HotelClock>();
builder.Services.AddRateLimiter(options=> {
    options.RejectionStatusCode=429;
    options.AddPolicy("accounts",context=>RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",_=>new FixedWindowRateLimiterOptions {
            PermitLimit=10,Window=TimeSpan.FromMinutes(1),QueueLimit=0,AutoReplenishment=true
        }));
});

var app=builder.Build();
if(setup)
{
    if(string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("HotelDatabase")))
        throw new InvalidOperationException("HotelDatabase baglantisi eksik. KURULUM.md adimlarini uygulayin.");
    await using var scope=app.Services.CreateAsyncScope();
    await DatabaseSetup.InitializeAsync(scope.ServiceProvider.GetRequiredService<HotelDbContext>(),
        scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>(),scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>(),builder.Configuration);
    Console.WriteLine("Kurulum tamamlandi. Oda ornekleri ve yonetici hesaplari hazir.");
    return;
}

// Invariant form parsing keeps decimal points and ISO dates consistent with HTML number/date controls.
var culture=CultureInfo.InvariantCulture;
app.UseForwardedHeaders();
app.UseRequestLocalization(new RequestLocalizationOptions {DefaultRequestCulture=new(culture),SupportedCultures=[culture],SupportedUICultures=[culture]});
app.UseExceptionHandler("/Home/Error");
if(!app.Environment.IsDevelopment()) { app.UseHsts(); app.UseHttpsRedirection(); }
app.Use(async(context,next)=> {
    context.Response.Headers["X-Content-Type-Options"]="nosniff";
    context.Response.Headers["Referrer-Policy"]="strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"]="default-src 'self'; img-src 'self' https:; style-src 'self'; script-src 'self'; object-src 'none'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'";
    context.Response.Headers["Cache-Control"]="no-store";
    await next();
});
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapGet("/health",()=>Results.Ok(new {status="ok"})).AllowAnonymous();
app.MapControllerRoute("default","{controller=Home}/{action=Index}/{id?}");
app.Run();

public partial class Program { }
