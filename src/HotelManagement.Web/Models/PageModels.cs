using HotelManagement.Application.DTOs;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace HotelManagement.Web.Models;

public record CatalogPage(List<RoomTypeDto> Types,List<ReviewDto> Reviews);
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
