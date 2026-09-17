using HotelManagement.Application.DTOs;
using HotelManagement.Domain.Entities;
namespace HotelManagement.Application.Mapping;

public static class HotelMapping
{
    public static RoomTypeDto ToDto(this RoomType x) => new(x.Id,x.Name,x.Description,x.BasePrice,x.Capacity,x.BedCount,x.SizeInSquareMeters,x.Amenities,x.ImageUrls,x.IsActive,
        x.IsFeatured,x.FeaturedOrder,x.Images.OrderBy(i=>i.SortOrder).ThenBy(i=>i.CreatedAtUtc)
            .Select(i=>new RoomImageDto(i.Id,$"/media/{i.MediaAssetId}",i.AltText,i.SortOrder)).ToList());
    public static RoomDto ToDto(this Room x) => new(x.Id,x.Number,x.Floor,x.RoomTypeId,x.RoomType.Name,x.RoomType.BasePrice,x.RoomType.Capacity,x.Status,x.IsActive);
    public static StayPackageDto ToDto(this StayPackage x) => new(x.Id,x.Name,x.Description,x.Benefits,x.PricePerNight,x.IsActive,x.SortOrder);
    public static ReservationDto ToDto(this Reservation x, bool reviewed) => new(x.Id,x.Code,x.CustomerId,x.RoomNumber,x.RoomTypeName,x.CheckInDate,x.CheckOutDate,x.GuestCount,x.GuestName,x.GuestPhone,x.TotalPrice,x.Status,reviewed,
        x.StayPackageId,x.PackageName,x.PackageDescription,x.PackageBenefits,x.NightlyPrice,x.PackagePricePerNight,x.RoomSubtotal,x.PackageSubtotal);
    public static ReservationEventDto ToDto(this ReservationEvent x) => new(x.Id,x.Status,x.Title,x.Description,x.Actor,x.CreatedAtUtc);
    public static ReviewDto ToDto(this Review x) => new(x.Id,x.Reservation.RoomTypeId,x.Reservation.RoomTypeName,x.Rating,x.Comment,x.IsApproved,x.CreatedAtUtc);
    public static JobDto ToDto(this RoomJob x) => new(x.Id,x.RoomId,x.Room.Number,x.Kind,x.Description,x.CreatedAtUtc,x.CompletedAtUtc,x.CompletedBy);
}
