using HotelManagement.Domain.Common;
namespace HotelManagement.Domain.Entities;

public class Review : BaseEntity
{
    public Guid ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;
    public int Rating { get; set; }
    public string Comment { get; set; } = "";
    public bool IsApproved { get; set; }
}
