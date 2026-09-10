using System.ComponentModel.DataAnnotations;
namespace HotelManagement.Application.DTOs;

public class LoginInput
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
    public bool Staff { get; set; }
}
public class RegisterInput
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    [Required, EmailAddress, StringLength(254)] public string Email { get; set; } = "";
    [Required, StringLength(128, MinimumLength=12), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Compare(nameof(Password)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}
public class StaffInput : RegisterInput
{
    [Required] public string Role { get; set; } = "Reception";
}
public record AccountDto(Guid Id, string FullName, string Email, string Role);
public interface IAccountService
{
    Task<bool> LoginAsync(LoginInput input);
    Task RegisterAsync(RegisterInput input);
    Task LogoutAsync();
    Task<List<AccountDto>> UsersAsync();
    Task CreateStaffAsync(StaffInput input);
    Task ChangeRoleAsync(Guid id, string role, Guid actorId);
}
