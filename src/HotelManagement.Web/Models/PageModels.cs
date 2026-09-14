using HotelManagement.Application.DTOs;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace HotelManagement.Web.Models;

public record CatalogPage(List<RoomTypeDto> Types,List<ReviewDto> Reviews);
public record NewsItem(string Slug,string Category,string Title,string Summary,string Body,DateOnly PublishedAt,int ReadingMinutes);

public static class SiteContent
{
    public static readonly IReadOnlyList<NewsItem> News=
    [
        new("sehirde-yavas-bir-hafta-sonu","Şehir Rehberi","Şehirde yavaş bir hafta sonu",
            "Koşturmayı bırakıp İstanbul'un sakin köşelerini keşfetmek için küçük bir Meridian rotası.",
            "Güne telaşsız bir kahvaltıyla başlayın. Moda sahilinde kısa bir yürüyüşten sonra Yeldeğirmeni'nin ara sokaklarına geçin; küçük dükkânlar, kahve molaları ve tarihi apartmanlar günün temposunu kendiliğinden düşürür.\n\nAkşamüstünü gün batımına ayırın. Otelinize döndüğünüzde resepsiyon ekibimiz yakın çevredeki güncel önerileri paylaşabilir. En iyi şehir planı bazen en az duraklı olandır.",
            new(2026,9,12),3),
        new("meridian-kahvalti-ritueli","Lezzet","Meridian kahvaltı ritüeli",
            "Yerel ürünler, mevsim tatları ve uzun sohbetlere yakışan sade bir sabah masası.",
            "Kahvaltımızda gösterişten çok iyi ürüne yer açıyoruz. Mevsim meyveleri, günlük ekmekler, yerel peynirler ve mutfağımızdan çıkan sıcak tabaklar küçük porsiyonlarla masaya geliyor.\n\nAlerjen veya özel beslenme tercihiniz varsa varıştan önce bize ulaşmanız yeterli. Ekibimiz uygun seçenekleri önceden planlar.",
            new(2026,9,8),2),
        new("konaklamanizi-kolaylastiran-yenilikler","Meridian'dan","Konaklamanızı kolaylaştıran yenilikler",
            "Rezervasyondan çıkışa kadar daha açık, hızlı ve sakin bir dijital misafir deneyimi.",
            "Yeni misafir alanımız ile rezervasyon taleplerinizi tek ekrandan takip edebilir, yaklaşan konaklamanızı görebilir ve tamamlanan ziyaretinizi değerlendirebilirsiniz.\n\nMüsaitlik ekranı tarih ve kişi sayısına göre uygun odaları karşılaştırır. Her adımda toplam fiyatı görürsünüz; sürpriz ücret yoktur.",
            new(2026,9,1),2)
    ];
}

public class ContactInput
{
    [Required(ErrorMessage="Adınızı yazın."),StringLength(100)] public string FullName {get;set;}="";
    [Required(ErrorMessage="E-posta adresinizi yazın."),EmailAddress(ErrorMessage="Geçerli bir e-posta adresi yazın."),StringLength(254)] public string Email {get;set;}="";
    [StringLength(30)] public string Phone {get;set;}="";
    [Required(ErrorMessage="Bir konu seçin."),StringLength(50)] public string Subject {get;set;}="";
    [Required(ErrorMessage="Mesajınızı yazın."),StringLength(1500,MinimumLength=10,ErrorMessage="Mesaj en az 10, en fazla 1500 karakter olmalı.")] public string Message {get;set;}="";
}
public class SearchPage
{
    public DateOnly Start {get;set;}
    public DateOnly End {get;set;}
    public int Guests {get;set;}=2;
    public bool Searched {get;set;}
    public List<RoomDto> Rooms {get;set;}=[];
}
public record DetailPage(RoomTypeDto Type,List<RoomDto> Rooms,List<ReviewDto> Reviews);
public class BookingPage
{
    public BookingInput Input {get;set;}=new();
    [BindNever,ValidateNever] public RoomDto Room {get;set;}=null!;
    [BindNever,ValidateNever] public List<BusyPeriod> Busy {get;set;}=[];
}
public class RoomEditPage
{
    public RoomInput Input {get;set;}=new();
    [BindNever,ValidateNever] public List<RoomTypeDto> Types {get;set;}=[];
}
public class RoomManagementPage
{
    public BulkRoomInput Bulk {get;set;}=new();
    [BindNever,ValidateNever] public List<RoomDto> Rooms {get;set;}=[];
    [BindNever,ValidateNever] public List<RoomTypeDto> Types {get;set;}=[];
}
public class JobPage
{
    public JobInput Input {get;set;}=new();
    [BindNever,ValidateNever] public List<RoomDto> Rooms {get;set;}=[];
    [BindNever,ValidateNever] public List<JobDto> Jobs {get;set;}=[];
}
public class UsersPage
{
    public StaffInput Input {get;set;}=new();
    [BindNever,ValidateNever] public List<AccountDto> Users {get;set;}=[];
}

public class StaffBookingPage
{
    public Guid CustomerId {get;set;}
    public BookingInput Input {get;set;}=new();
    [BindNever,ValidateNever] public List<RoomDto> Rooms {get;set;}=[];
    [BindNever,ValidateNever] public List<AccountDto> Customers {get;set;}=[];
}
