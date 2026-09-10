using System.Globalization;
using HotelManagement.Domain.Enums;
namespace HotelManagement.Web.Helpers;
public static class Ui
{
    public static string Money(decimal price)=>price.ToString("N2",CultureInfo.GetCultureInfo("tr-TR"))+" ₺";
    public static string Status(ReservationStatus status)=>status switch {
        ReservationStatus.Pending=>"Onay bekliyor",ReservationStatus.Confirmed=>"Onaylandı",ReservationStatus.CheckedIn=>"Konaklıyor",
        ReservationStatus.CheckedOut=>"Tamamlandı",ReservationStatus.Cancelled=>"İptal",ReservationStatus.Rejected=>"Reddedildi",_=>"Bilinmiyor"};
    public static string Status(RoomStatus status)=>status switch {
        RoomStatus.Available=>"Hazır",RoomStatus.Occupied=>"Dolu",RoomStatus.Cleaning=>"Temizlikte",RoomStatus.Maintenance=>"Bakımda",_=>"Hizmet dışı"};
    public static string Role(string role)=>role switch {"Customer"=>"Müşteri","Admin"=>"Oda yöneticisi","SuperAdmin"=>"Süper admin",
        "Reception"=>"Resepsiyon","Cleaner"=>"Temizlik görevlisi","Maintenance"=>"Teknik servis",_=>role};
    public static string[] Images(string urls)=>urls.Split('\n',StringSplitOptions.TrimEntries|StringSplitOptions.RemoveEmptyEntries)
        .Where(u=>Uri.TryCreate(u,UriKind.Absolute,out var uri)&&uri.Scheme=="https").ToArray();
}
