using Microsoft.AspNetCore.Identity;
namespace HotelManagement.Infrastructure.Identity;
public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = "";
}
