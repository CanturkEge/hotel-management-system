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
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<RoomTypeImage> RoomTypeImages => Set<RoomTypeImage>();
    public DbSet<NewsArticle> NewsArticles => Set<NewsArticle>();
    public DbSet<HomePageContent> HomePageContents => Set<HomePageContent>();

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
            e.HasIndex(x=>new{x.IsFeatured,x.FeaturedOrder});
            e.Navigation(x=>x.Images).AutoInclude();
            e.ToTable("RoomTypes", t=>t.HasCheckConstraint("CK_RoomType_Values", "\"BasePrice\" > 0 AND \"Capacity\" > 0 AND \"BedCount\" > 0 AND \"SizeInSquareMeters\" > 0 AND \"FeaturedOrder\" >= 0"));
        });
        b.Entity<MediaAsset>(e=> {
            e.Property(x=>x.FileName).HasMaxLength(180); e.Property(x=>x.ContentType).HasMaxLength(32);
            e.Property(x=>x.Sha256).HasMaxLength(64); e.HasIndex(x=>x.Sha256);
            e.ToTable("MediaAssets",t=>t.HasCheckConstraint("CK_MediaAsset_Length","\"Length\" > 0 AND \"Length\" <= 5242880"));
        });
        b.Entity<RoomTypeImage>(e=> {
            e.Property(x=>x.AltText).HasMaxLength(180); e.HasIndex(x=>new{x.RoomTypeId,x.SortOrder});
            e.HasOne(x=>x.RoomType).WithMany(x=>x.Images).HasForeignKey(x=>x.RoomTypeId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne<MediaAsset>().WithMany().HasForeignKey(x=>x.MediaAssetId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable("RoomTypeImages",t=>t.HasCheckConstraint("CK_RoomTypeImage_Order","\"SortOrder\" >= 0"));
        });
        b.Entity<NewsArticle>(e=> {
            e.Property(x=>x.Slug).HasMaxLength(120); e.HasIndex(x=>x.Slug).IsUnique();
            e.Property(x=>x.Category).HasMaxLength(80); e.Property(x=>x.Title).HasMaxLength(180);
            e.Property(x=>x.Summary).HasMaxLength(500); e.Property(x=>x.Body).HasMaxLength(12000);
            e.HasIndex(x=>new{x.IsPublished,x.IsFeatured,x.SortOrder,x.PublishedAt});
            e.HasOne<MediaAsset>().WithMany().HasForeignKey(x=>x.CoverImageId).OnDelete(DeleteBehavior.Restrict);
            e.ToTable("NewsArticles",t=>t.HasCheckConstraint("CK_NewsArticle_Values","\"ReadingMinutes\" BETWEEN 1 AND 60 AND \"SortOrder\" >= 0"));
        });
        b.Entity<HomePageContent>(e=> {
            e.Property(x=>x.HeroEyebrow).HasMaxLength(100); e.Property(x=>x.HeroTitle).HasMaxLength(180); e.Property(x=>x.HeroText).HasMaxLength(600); e.Property(x=>x.HeroCaption).HasMaxLength(160);
            e.Property(x=>x.RoomsEyebrow).HasMaxLength(100); e.Property(x=>x.RoomsTitle).HasMaxLength(180); e.Property(x=>x.RoomsText).HasMaxLength(600);
            e.Property(x=>x.ExperienceEyebrow).HasMaxLength(100); e.Property(x=>x.ExperienceTitle).HasMaxLength(180); e.Property(x=>x.ExperienceText).HasMaxLength(600);
            e.Property(x=>x.ExperienceOneTitle).HasMaxLength(100); e.Property(x=>x.ExperienceOneText).HasMaxLength(500);
            e.Property(x=>x.ExperienceTwoTitle).HasMaxLength(100); e.Property(x=>x.ExperienceTwoText).HasMaxLength(500);
            e.Property(x=>x.ExperienceThreeTitle).HasMaxLength(100); e.Property(x=>x.ExperienceThreeText).HasMaxLength(500);
            e.Property(x=>x.NewsEyebrow).HasMaxLength(100); e.Property(x=>x.NewsTitle).HasMaxLength(180);
            e.Property(x=>x.ContactEyebrow).HasMaxLength(100); e.Property(x=>x.ContactTitle).HasMaxLength(180); e.Property(x=>x.ContactText).HasMaxLength(600);
            e.Property(x=>x.ContactPhone).HasMaxLength(30); e.Property(x=>x.ContactEmail).HasMaxLength(254);
            e.HasOne<MediaAsset>().WithMany().HasForeignKey(x=>x.HeroImageId).OnDelete(DeleteBehavior.Restrict);
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
