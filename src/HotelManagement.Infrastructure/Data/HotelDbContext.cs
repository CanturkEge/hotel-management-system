using HotelManagement.Domain.Common;
using HotelManagement.Domain.Entities;
using HotelManagement.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace HotelManagement.Infrastructure.Data;

public sealed class HotelDbContext(DbContextOptions<HotelDbContext> options) : IdentityDbContext<AppUser,IdentityRole<Guid>,Guid>(options)
{
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<RoomJob> RoomJobs => Set<RoomJob>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        // Do not expose this schema in Supabase Data API settings.
        b.HasDefaultSchema("hotel");
        b.Entity<AppUser>().Property(x=>x.FullName).HasMaxLength(100);
        b.Entity<AppUser>().HasIndex(x=>x.NormalizedEmail).IsUnique();
        b.Entity<RoomType>(e=> {
            e.Property(x=>x.Name).HasMaxLength(100); e.Property(x=>x.Description).HasMaxLength(4000);
            e.Property(x=>x.Amenities).HasMaxLength(1000); e.Property(x=>x.ImageUrls).HasMaxLength(8000);
            e.Property(x=>x.BasePrice).HasPrecision(12,2);
            e.ToTable("RoomTypes", t=>t.HasCheckConstraint("CK_RoomType_Values", "\"BasePrice\" > 0 AND \"Capacity\" > 0 AND \"BedCount\" > 0 AND \"SizeInSquareMeters\" > 0"));
        });
        b.Entity<Room>(e=> {
            e.Property(x=>x.Number).HasMaxLength(12); e.HasIndex(x=>x.Number).IsUnique();
            e.HasOne(x=>x.RoomType).WithMany().HasForeignKey(x=>x.RoomTypeId).OnDelete(DeleteBehavior.Restrict);
            e.Navigation(x=>x.RoomType).AutoInclude();
        });
        b.Entity<Reservation>(e=> {
            e.HasIndex(x=>x.Code).IsUnique(); e.Property(x=>x.Code).HasMaxLength(32);
            e.HasIndex(x=>new{x.RoomId,x.Status,x.CheckInDate,x.CheckOutDate});
            e.HasIndex(x=>new{x.CustomerId,x.CreatedAtUtc});
            e.Property(x=>x.GuestName).HasMaxLength(100); e.Property(x=>x.GuestPhone).HasMaxLength(30);
            e.Property(x=>x.Currency).HasMaxLength(3);
            e.Property(x=>x.RoomTypeName).HasMaxLength(100); e.Property(x=>x.RoomNumber).HasMaxLength(12);
            e.Property(x=>x.NightlyPrice).HasPrecision(12,2); e.Property(x=>x.TotalPrice).HasPrecision(14,2);
            e.HasOne(x=>x.Room).WithMany().HasForeignKey(x=>x.RoomId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<RoomType>().WithMany().HasForeignKey(x=>x.RoomTypeId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<AppUser>().WithMany().HasForeignKey(x=>x.CustomerId).OnDelete(DeleteBehavior.Restrict);
            e.Navigation(x=>x.Room).AutoInclude();
            e.ToTable("Reservations",t=>t.HasCheckConstraint("CK_Reservation_Values", "\"CheckOutDate\" > \"CheckInDate\" AND \"GuestCount\" > 0 AND \"TotalPrice\" >= 0"));
        });
        b.Entity<Review>(e=> {
            e.HasIndex(x=>x.ReservationId).IsUnique(); e.Property(x=>x.Comment).HasMaxLength(1500);
            e.HasOne(x=>x.Reservation).WithMany().HasForeignKey(x=>x.ReservationId).OnDelete(DeleteBehavior.Restrict);
            e.Navigation(x=>x.Reservation).AutoInclude();
            e.ToTable("Reviews",t=>t.HasCheckConstraint("CK_Review_Rating", "\"Rating\" BETWEEN 1 AND 5"));
        });
        b.Entity<RoomJob>(e=> {
            e.HasIndex(x=>new{x.RoomId,x.CompletedAtUtc}); e.Property(x=>x.Description).HasMaxLength(1000);
            e.HasOne(x=>x.Room).WithMany().HasForeignKey(x=>x.RoomId).OnDelete(DeleteBehavior.Restrict);
            e.Navigation(x=>x.Room).AutoInclude();
        });
    }
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken=default)
    {
        foreach(var entry in ChangeTracker.Entries<BaseEntity>().Where(x=>x.State==EntityState.Modified))
            entry.Entity.UpdatedAtUtc=DateTime.UtcNow;
        return base.SaveChangesAsync(cancellationToken);
    }
}
