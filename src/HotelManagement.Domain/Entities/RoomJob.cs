using HotelManagement.Domain.Common;
using HotelManagement.Domain.Enums;
namespace HotelManagement.Domain.Entities;

public class RoomJob : BaseEntity
{
    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public JobKind Kind { get; set; }
    public string Description { get; set; } = "";
    public DateTime? CompletedAtUtc { get; set; }
    public string? CompletedBy { get; set; }
}
