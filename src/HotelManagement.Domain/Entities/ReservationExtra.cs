using HotelManagement.Domain.Common;
namespace HotelManagement.Domain.Entities;

public class ReservationExtra : BaseEntity
{
    public Guid ReservationId { get; set; }
    public Reservation Reservation { get; set; } = null!;
    public Guid ExtraServiceId { get; set; }
    public ExtraService ExtraService { get; set; } = null!;
    public string ServiceName { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal TotalPrice { get; set; }
}
