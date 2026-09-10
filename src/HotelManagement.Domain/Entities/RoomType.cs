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
    // One HTTPS image address per line; rendered by the browser, never fetched by the server.
    public string ImageUrls { get; set; } = "";
    public bool IsActive { get; set; } = true;
}
