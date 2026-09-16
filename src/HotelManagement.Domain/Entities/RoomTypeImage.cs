using HotelManagement.Domain.Common;
namespace HotelManagement.Domain.Entities;

public class RoomTypeImage : BaseEntity
{
    public Guid RoomTypeId { get; set; }
    public RoomType RoomType { get; set; } = null!;
    public Guid MediaAssetId { get; set; }
    public string AltText { get; set; } = "";
    public int SortOrder { get; set; }
}
