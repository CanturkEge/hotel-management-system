using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace HotelManagement.Infrastructure.Data;

public class DesignTimeFactory : IDesignTimeDbContextFactory<HotelDbContext>
{
    public HotelDbContext CreateDbContext(string[] args)
    {
        // Used only for generating migrations; actual DB updates are executed by Web --setup.
        var options=new DbContextOptionsBuilder<HotelDbContext>().UseNpgsql(
            "Host=localhost;Database=hotel_design_only;Username=postgres",
            x=>x.MigrationsHistoryTable("__EFMigrationsHistory","hotel")).Options;
        return new HotelDbContext(options);
    }
}
