using HotelManagement.Domain.Common;
using HotelManagement.Domain.Enums;
namespace HotelManagement.Domain.Entities;

public class ReservationEvent : BaseEntity
{
    public Guid ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;
    public ReservationStatus Status { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Actor { get; set; } = "";
}
