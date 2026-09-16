using HotelManagement.Application.DTOs;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace HotelManagement.Web.Models;

public record CatalogPage(List<RoomTypeDto> Types,List<ReviewDto> Reviews,HomePageDto Home,List<NewsArticleDto> News);

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
public class RoomTypeEditorPage
{
    public RoomTypeInput Input {get;set;}=new();
    [BindNever,ValidateNever] public List<RoomImageDto> ExistingImages {get;set;}=[];
    [ValidateNever] public List<IFormFile> Images {get;set;}=[];
    public List<Guid> RemoveImageIds {get;set;}=[];
}
public class ContentDashboardPage
{
    public HomePageDto Home {get;set;}=null!;
    public List<NewsArticleDto> News {get;set;}=[];
}
public class HomeContentEditorPage
{
    public HomePageInput Input {get;set;}=new();
    [BindNever,ValidateNever] public string? CurrentHeroImageUrl {get;set;}
    [ValidateNever] public IFormFile? HeroImage {get;set;}
    public bool RemoveHeroImage {get;set;}
}
public class NewsEditorPage
{
    public NewsArticleInput Input {get;set;}=new();
    [BindNever,ValidateNever] public string? CurrentCoverImageUrl {get;set;}
    [ValidateNever] public IFormFile? CoverImage {get;set;}
    public bool RemoveCoverImage {get;set;}
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
