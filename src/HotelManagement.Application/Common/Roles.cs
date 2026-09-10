namespace HotelManagement.Application.Common;
public static class Roles
{
    public const string Customer = "Customer";
    public const string Admin = "Admin";
    public const string SuperAdmin = "SuperAdmin";
    public const string Reception = "Reception";
    public const string Cleaner = "Cleaner";
    public const string Maintenance = "Maintenance";
    public const string Managers = Admin + "," + SuperAdmin;
    public const string Desk = Managers + "," + Reception;
    public const string Staff = Desk + "," + Cleaner + "," + Maintenance;
    public static readonly string[] All = [Customer, Admin, SuperAdmin, Reception, Cleaner, Maintenance];
}
