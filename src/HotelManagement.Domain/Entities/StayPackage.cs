using HotelManagement.Domain.Common;
namespace HotelManagement.Domain.Entities;

public class StayPackage : BaseEntity
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Benefits { get; set; } = "";
    public decimal PricePerNight { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
