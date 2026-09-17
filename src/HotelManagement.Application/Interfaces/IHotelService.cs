using HotelManagement.Application.DTOs;
using HotelManagement.Domain.Enums;
namespace HotelManagement.Application.Interfaces;

public interface IHotelService
{
    Task<List<RoomTypeDto>> TypesAsync(bool includeInactive = false);
    Task<Guid> SaveTypeAsync(RoomTypeInput input);
    Task<List<RoomDto>> RoomsAsync(bool includeInactive = false);
    Task SaveRoomAsync(RoomInput input);
    Task<int> CreateRoomsAsync(BulkRoomInput input);
    Task ArchiveRoomAsync(Guid id);
    Task<List<RoomDto>> AvailableAsync(DateOnly start, DateOnly end, int guests);
    Task<List<BusyPeriod>> BusyAsync(Guid roomId);
    Task<List<StayPackageDto>> PackagesAsync(bool includeInactive = false);
    Task<Guid> SavePackageAsync(StayPackageInput input);
    Task<Guid> BookAsync(Guid customerId, BookingInput input, string actor = "Müşteri");
    Task<List<ReservationDto>> BookingsAsync(Guid? customerId = null);
    Task<ReservationDetailsDto> BookingDetailsAsync(Guid id, Guid? customerId = null);
    Task ChangeBookingAsync(Guid id, ReservationStatus target, Guid? customerId = null, string actor = "Sistem");
    Task ReviewAsync(Guid customerId, ReviewInput input);
    Task<List<ReviewDto>> ReviewsAsync(bool includeUnapproved = false);
    Task ModerateAsync(Guid id, bool approve);
    Task<List<JobDto>> JobsAsync(JobKind? kind = null);
    Task ReportJobAsync(JobInput input);
    Task CompleteJobAsync(Guid id, JobKind? allowedKind, string actor);
    Task<DashboardDto> DashboardAsync();
}
