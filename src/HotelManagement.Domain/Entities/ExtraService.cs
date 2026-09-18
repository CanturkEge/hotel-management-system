using HotelManagement.Domain.Common;
namespace HotelManagement.Domain.Entities;

public class ExtraService : BaseEntity
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
