using System.ComponentModel.DataAnnotations;
using HotelManagement.Domain.Enums;
namespace HotelManagement.Application.DTOs;

public class RoomTypeInput
{
    public Guid Id { get; set; }
    [Required, StringLength(100)] public string Name { get; set; } = "";
    [Required, StringLength(4000)] public string Description { get; set; } = "";
    [Range(typeof(decimal), "1", "1000000")] public decimal BasePrice { get; set; }
    [Range(1, 20)] public int Capacity { get; set; } = 2;
    [Range(1, 20)] public int BedCount { get; set; } = 1;
    [Range(5, 1000)] public int SizeInSquareMeters { get; set; } = 25;
    [StringLength(1000)] public string? Amenities { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    [Range(0, 999)] public int FeaturedOrder { get; set; }
}

public class RoomInput
{
    public Guid Id { get; set; }
    [Required, StringLength(12)] public string Number { get; set; } = "";
    [Range(-5, 100)] public int Floor { get; set; }
    public Guid RoomTypeId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class BulkRoomInput
{
    public Guid RoomTypeId { get; set; }
    [Range(1, 999999)] public int StartNumber { get; set; } = 101;
    [Range(1, 100)] public int Count { get; set; } = 10;
    [Range(-5, 100)] public int Floor { get; set; } = 1;
}

public class BookingInput
{
    public Guid RoomId { get; set; }
    public Guid StayPackageId { get; set; }
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    [Range(1,20)] public int GuestCount { get; set; } = 1;
    [Required, StringLength(100)] public string GuestName { get; set; } = "";
    [Required, StringLength(30, MinimumLength = 7)] public string GuestPhone { get; set; } = "";
}

public class StayPackageInput
{
    public Guid Id { get; set; }
    [Required, StringLength(80)] public string Name { get; set; } = "";
    [Required, StringLength(600)] public string Description { get; set; } = "";
    [Required, StringLength(1200)] public string Benefits { get; set; } = "";
    [Range(typeof(decimal), "0", "1000000")] public decimal PricePerNight { get; set; }
    [Range(0, 999)] public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ReviewInput
{
    public Guid ReservationId { get; set; }
    [Range(1,5)] public int Rating { get; set; } = 5;
    [Required, StringLength(1500, MinimumLength=5)] public string Comment { get; set; } = "";
}

public class JobInput
{
    public Guid RoomId { get; set; }
    public JobKind Kind { get; set; } = JobKind.Maintenance;
    [Required, StringLength(1000, MinimumLength=3)] public string Description { get; set; } = "";
}

public record RoomImageDto(Guid Id, string Url, string AltText, int SortOrder);
public record RoomTypeDto(Guid Id, string Name, string Description, decimal BasePrice, int Capacity,
    int BedCount, int SizeInSquareMeters, string Amenities, string ImageUrls, bool IsActive,
    bool IsFeatured, int FeaturedOrder, IReadOnlyList<RoomImageDto> Images);
public record RoomDto(Guid Id, string Number, int Floor, Guid RoomTypeId, string TypeName,
    decimal Price, int Capacity, RoomStatus Status, bool IsActive);
public record StayPackageDto(Guid Id, string Name, string Description, string Benefits,
    decimal PricePerNight, bool IsActive, int SortOrder);
public record BusyPeriod(DateOnly Start, DateOnly End);
public record ReservationDto(Guid Id, string Code, Guid CustomerId, string RoomNumber, string TypeName,
    DateOnly CheckInDate, DateOnly CheckOutDate, int GuestCount, string GuestName, string GuestPhone,
    decimal TotalPrice, ReservationStatus Status, bool HasReview, Guid? StayPackageId, string PackageName,
    string PackageDescription, string PackageBenefits, decimal NightlyPrice, decimal PackagePricePerNight,
    decimal RoomSubtotal, decimal PackageSubtotal);
public record ReservationEventDto(Guid Id, ReservationStatus Status, string Title, string Description,
    string Actor, DateTime CreatedAtUtc);
public record ReservationDetailsDto(ReservationDto Reservation, IReadOnlyList<ReservationEventDto> Events);
public record ReviewDto(Guid Id, Guid RoomTypeId, string TypeName, int Rating, string Comment, bool IsApproved, DateTime CreatedAtUtc);
public record JobDto(Guid Id, Guid RoomId, string RoomNumber, JobKind Kind, string Description,
    DateTime CreatedAtUtc, DateTime? CompletedAtUtc, string? CompletedBy);
public record DashboardDto(int ActiveRooms, int OccupiedRooms, int PendingBookings, int OpenJobs, decimal CompletedStayRevenue);
