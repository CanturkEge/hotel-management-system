using System.ComponentModel.DataAnnotations;
using HotelManagement.Application.Common;
using HotelManagement.Application.DTOs;
using HotelManagement.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace HotelManagement.Infrastructure.Identity;

public sealed class AccountService(UserManager<AppUser> users,SignInManager<AppUser> signIn,IUnitOfWork unit) : IAccountService
{
    private static readonly Guid AccountLock=Guid.Parse("9544aa4e-3d24-4df9-90b8-5053444ff4ef");
    private static void Check(IdentityResult result)
    {
        if(!result.Succeeded) throw new AppException(string.Join(" ",result.Errors.Select(x=>x.Description)));
    }
    public async Task<bool> LoginAsync(LoginInput input)
    {
        var user=await users.FindByEmailAsync(input.Email.Trim());
        if(user==null) return false;
        var check=await signIn.CheckPasswordSignInAsync(user,input.Password,lockoutOnFailure:true);
        if(!check.Succeeded) return false;
        var isCustomer=await users.IsInRoleAsync(user,Roles.Customer);
        if(input.Staff==isCustomer) return false;
        await signIn.SignInAsync(user,isPersistent:false);
        return true;
    }
    public Task LogoutAsync()=>signIn.SignOutAsync();
    public Task RegisterAsync(RegisterInput input)=>Create(input,Roles.Customer);
    public Task CreateStaffAsync(StaffInput input)
    {
        if(!Roles.All.Contains(input.Role) || input.Role is Roles.Customer or Roles.SuperAdmin)
            throw new AppException("Bu rol için personel hesabı oluşturulamaz.");
        return Create(input,input.Role);
    }
    private Task Create(RegisterInput input,string role)=>unit.LockedAsync(AccountLock,async ()=>
    {
        var results=new List<ValidationResult>();
        if(!Validator.TryValidateObject(input,new ValidationContext(input),results,true))
            throw new AppException(string.Join(" ",results.Select(x=>x.ErrorMessage)));
        if(role==Roles.Admin && (await users.GetUsersInRoleAsync(Roles.Admin)).Count>0)
            throw new AppException("Sistemde zaten bir oda yöneticisi var.");
        var user=new AppUser{Id=Guid.NewGuid(),UserName=input.Email.Trim(),Email=input.Email.Trim(),FullName=input.FullName.Trim()};
        Check(await users.CreateAsync(user,input.Password));
        Check(await users.AddToRoleAsync(user,role));
        return true;
    });
    public async Task<List<AccountDto>> UsersAsync()
    {
        var result=new List<AccountDto>();
        foreach(var user in await users.Users.AsNoTracking().OrderBy(x=>x.FullName).ToListAsync())
            result.Add(new(user.Id,user.FullName,user.Email ?? "",(await users.GetRolesAsync(user)).FirstOrDefault() ?? ""));
        return result;
    }
    public Task ChangeRoleAsync(Guid id,string role,Guid actorId)=>unit.LockedAsync(AccountLock,async ()=>
    {
        if(!Roles.All.Contains(role) || role is Roles.SuperAdmin or Roles.Customer) throw new AppException("Geçersiz personel rolü.");
        var user=await users.FindByIdAsync(id.ToString()) ?? throw new AppException("Kullanıcı bulunamadı.");
        var current=await users.GetRolesAsync(user);
        if(id==actorId || current.Contains(Roles.SuperAdmin) || current.Contains(Roles.Customer))
            throw new AppException("Kendi hesabınızın, süper adminin veya müşterinin rolü bu ekrandan değiştirilemez.");
        if(current.Contains(Roles.Admin) && role!=Roles.Admin) throw new AppException("Tek oda yöneticisinin rolü değiştirilemez.");
        if(role==Roles.Admin && !current.Contains(Roles.Admin) && (await users.GetUsersInRoleAsync(Roles.Admin)).Count>0)
            throw new AppException("Zaten bir oda yöneticisi var.");
        Check(await users.RemoveFromRolesAsync(user,current));
        Check(await users.AddToRoleAsync(user,role));
        Check(await users.UpdateSecurityStampAsync(user));
        return true;
    });
}
