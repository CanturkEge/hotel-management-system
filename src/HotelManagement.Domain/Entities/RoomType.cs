using HotelManagement.Domain.Common;
namespace HotelManagement.Domain.Entities;

public class RoomType : BaseEntity
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal BasePrice { get; set; }
    public int Capacity { get; set; }
    public int BedCount { get; set; }
    public int SizeInSquareMeters { get; set; }
    public string Amenities { get; set; } = "";
    // Kept for the bundled, repository-owned starter images. New images use RoomTypeImage.
    public string ImageUrls { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int FeaturedOrder { get; set; }
    public List<RoomTypeImage> Images { get; set; } = [];
}
