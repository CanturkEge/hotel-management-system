using HotelManagement.Domain.Common;
using HotelManagement.Domain.Enums;
namespace HotelManagement.Domain.Entities;

public class Reservation : BaseEntity
{
    public string Code { get; set; } = Guid.NewGuid().ToString("N").ToUpperInvariant();
    public Guid CustomerId { get; set; }
    public Guid RoomId { get; set; }
    public Room Room { get; set; } = null!;
    public Guid RoomTypeId { get; set; }
    public string RoomTypeName { get; set; } = "";
    public string RoomNumber { get; set; } = "";
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public int GuestCount { get; set; }
    public string GuestName { get; set; } = "";
    public string GuestPhone { get; set; } = "";
    public decimal NightlyPrice { get; set; }
    public Guid? StayPackageId { get; set; }
    public StayPackage? StayPackage { get; set; }
    public string PackageName { get; set; } = "Standart";
    public string PackageDescription { get; set; } = "Temel konaklama paketi";
    public string PackageBenefits { get; set; } = "Oda konaklaması";
    public decimal PackagePricePerNight { get; set; }
    public decimal RoomSubtotal { get; set; }
    public decimal PackageSubtotal { get; set; }
    public decimal ServicesSubtotal { get; set; }
    public Guid? PromotionId { get; set; }
    public Promotion? Promotion { get; set; }
    public string PromotionCode { get; set; } = "";
    public decimal DiscountAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "TRY";
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public DateTime? ActualCheckInUtc { get; set; }
    public DateTime? ActualCheckOutUtc { get; set; }
    public List<ReservationEvent> Events { get; set; } = [];
    public List<ReservationExtra> Extras { get; set; } = [];
}
