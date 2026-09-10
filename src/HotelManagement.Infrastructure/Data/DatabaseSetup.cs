using HotelManagement.Application.Common;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace HotelManagement.Infrastructure.Data;

public static class DatabaseSetup
{
    public static async Task InitializeAsync(HotelDbContext db,UserManager<AppUser> users,RoleManager<IdentityRole<Guid>> roles,IConfiguration config)
    {
        if(!db.Database.GetMigrations().Any()) throw new InvalidOperationException("Migration yok. Once scripts/Setup.ps1 calistirin.");
        await db.Database.MigrateAsync();
        await using var transaction=await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(482948239)");
        foreach(var role in Roles.All)
            if(!await roles.RoleExistsAsync(role)) Ensure(await roles.CreateAsync(new IdentityRole<Guid>(role)));
        await SeedUser("SuperAdmin",Roles.SuperAdmin,"Genel Yönetici");
        await SeedUser("Admin",Roles.Admin,"Oda Yöneticisi");
        if(!await db.RoomTypes.AnyAsync())
        {
            var type=new RoomType {Name="Deluxe Oda",Description="Aydınlık yaşam alanı, geniş çift kişilik yatak ve dinlenme köşesi.",
                BasePrice=3500,Capacity=2,BedCount=1,SizeInSquareMeters=32,Amenities="Wi-Fi, Klima, Çalışma masası, Duş, Minibar"};
            db.RoomTypes.Add(type);
            db.Rooms.AddRange(new Room {Number="101",Floor=1,RoomTypeId=type.Id},new Room {Number="102",Floor=1,RoomTypeId=type.Id});
            await db.SaveChangesAsync();
        }
        // Private schema: no Data API grants. Enable RLS as defense in depth.
        // Runtime connects through the configured database owner; authorization is enforced by C#.
        await db.Database.ExecuteSqlRawAsync("""
            REVOKE ALL ON SCHEMA hotel FROM PUBLIC, anon, authenticated;
            REVOKE ALL ON ALL TABLES IN SCHEMA hotel FROM PUBLIC, anon, authenticated;
            DO $$ DECLARE t record; BEGIN
                FOR t IN SELECT tablename FROM pg_tables WHERE schemaname='hotel' LOOP
                    EXECUTE format('ALTER TABLE hotel.%I ENABLE ROW LEVEL SECURITY',t.tablename);
                END LOOP;
            END $$;
            """);
        await transaction.CommitAsync();

        async Task SeedUser(string section,string role,string name)
        {
            if((await users.GetUsersInRoleAsync(role)).Count>0) return;
            var email=config[$"Seed:{section}:Email"];
            var password=config[$"Seed:{section}:Password"];
            if(string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException($"Seed:{section}:Email ve Password User Secrets icinde gerekli.");
            if(await users.FindByEmailAsync(email)!=null) throw new InvalidOperationException("Baslangic yoneticisi e-postasi baska hesapta kullaniliyor.");
            var user=new AppUser{Id=Guid.NewGuid(),UserName=email,Email=email,FullName=name};
            Ensure(await users.CreateAsync(user,password)); Ensure(await users.AddToRoleAsync(user,role));
        }
    }
    private static void Ensure(IdentityResult result)
    {
        if(!result.Succeeded) throw new InvalidOperationException(string.Join(" ",result.Errors.Select(x=>x.Description)));
    }
}
