using System.Security.Claims;
using HotelManagement.Application.Common;
using HotelManagement.Application.DTOs;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Enums;
using HotelManagement.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HotelManagement.Web.Controllers;

[Authorize(Roles=Roles.Customer)]
public class BookingsController(IHotelService hotel,IHotelClock clock) : Controller
{
    private Guid CustomerId=>Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    public async Task<IActionResult> Index()=>View(await hotel.BookingsAsync(CustomerId));
    [HttpGet]
    public async Task<IActionResult> Create(Guid roomId,DateOnly? start,DateOnly? end,int guests=1)
    {
        var room=(await hotel.RoomsAsync()).FirstOrDefault(x=>x.Id==roomId);
        if(room==null) return NotFound();
        var packages=await hotel.PackagesAsync();
        return View(new BookingPage{Room=room,Busy=await hotel.BusyAsync(roomId),Packages=packages,Input=new BookingInput{RoomId=roomId,
            StayPackageId=packages.FirstOrDefault()?.Id??Guid.Empty,CheckInDate=start??clock.Today,CheckOutDate=end??clock.Today.AddDays(1),GuestCount=guests}});
    }
    [HttpPost]
    public async Task<IActionResult> Create(BookingPage page)
    {
        // Room/Busy are display-only, populated server-side after validation.
        ModelState.Remove(nameof(page.Room));
        if(ModelState.IsValid)
            try {await hotel.BookAsync(CustomerId,page.Input,User.Identity?.Name??"Müşteri");TempData["Success"]="Talebiniz resepsiyon onayına gönderildi.";return RedirectToAction(nameof(Index));}
            catch(AppException ex){ModelState.AddModelError("",ex.Message);}
        var room=(await hotel.RoomsAsync()).FirstOrDefault(x=>x.Id==page.Input.RoomId);
        if(room==null) return NotFound();
        page.Room=room;page.Busy=await hotel.BusyAsync(room.Id);page.Packages=await hotel.PackagesAsync();return View(page);
    }
    [HttpGet] public async Task<IActionResult> Details(Guid id)
    {
        try{return View(await hotel.BookingDetailsAsync(id,CustomerId));}
        catch(AppException){return NotFound();}
    }
    [HttpPost] public async Task<IActionResult> Cancel(Guid id)
    {
        try {await hotel.ChangeBookingAsync(id,ReservationStatus.Cancelled,CustomerId,User.Identity?.Name??"Müşteri");TempData["Success"]="Rezervasyon iptal edildi.";}
        catch(AppException ex){TempData["Error"]=ex.Message;}
        return RedirectToAction(nameof(Index));
    }
    [HttpPost] public async Task<IActionResult> Review(ReviewInput input)
    {
        try {await hotel.ReviewAsync(CustomerId,input);TempData["Success"]="Yorumunuz yönetici onayına gönderildi.";}
        catch(AppException ex){TempData["Error"]=ex.Message;}
        return RedirectToAction(nameof(Index));
    }
}
