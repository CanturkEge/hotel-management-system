using HotelManagement.Domain.Enums;
namespace HotelManagement.Domain.Rules;

public static class BookingRules
{
    public static bool Overlaps(DateOnly start, DateOnly end, DateOnly otherStart, DateOnly otherEnd)
        => start < otherEnd && end > otherStart;

    public static bool BlocksDates(ReservationStatus status)
        => status is ReservationStatus.Confirmed or ReservationStatus.CheckedIn;

    public static decimal Total(DateOnly start, DateOnly end, decimal nightlyPrice)
    {
        if (end <= start || nightlyPrice < 0) throw new ArgumentException("Geçersiz tarih veya fiyat.");
        return (end.DayNumber - start.DayNumber) * nightlyPrice;
    }
}
