using HotelManagement.Application.Interfaces;
namespace HotelManagement.Infrastructure;
public sealed class HotelClock : IHotelClock
{
    private readonly TimeZoneInfo zone=TimeZoneInfo.FindSystemTimeZoneById("Europe/Istanbul");
    public DateTime UtcNow=>DateTime.UtcNow;
    public DateOnly Today=>DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(UtcNow,zone));
}
