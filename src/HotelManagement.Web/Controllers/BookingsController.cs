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
    [HttpGet] public async Task<IActionResult> Edit(Guid id)
    {
        try
        {
            var detail=await hotel.BookingDetailsAsync(id,CustomerId);var r=detail.Reservation;
            if(r.Status is not (ReservationStatus.Pending or ReservationStatus.Confirmed)){TempData["Error"]="Bu rezervasyon artık düzenlenemez.";return RedirectToAction(nameof(Details),new{id});}
            return View(new BookingEditPage {Rooms=await hotel.RoomsAsync(),Packages=await hotel.PackagesAsync(),Input=new ReservationEditInput {
                Id=r.Id,RoomId=(await hotel.RoomsAsync()).FirstOrDefault(x=>x.Number==r.RoomNumber)?.Id??Guid.Empty,StayPackageId=r.StayPackageId??Guid.Empty,
                CheckInDate=r.CheckInDate,CheckOutDate=r.CheckOutDate,GuestCount=r.GuestCount,GuestName=r.GuestName,GuestPhone=r.GuestPhone}});
        }
        catch(AppException){return NotFound();}
    }
    [HttpPost] public async Task<IActionResult> Edit(BookingEditPage page)
    {
        if(ModelState.IsValid)
            try {await hotel.UpdateBookingAsync(page.Input,CustomerId,User.Identity?.Name??"Müşteri");TempData["Success"]="Rezervasyon ve ücret bilgileri güncellendi.";return RedirectToAction(nameof(Details),new{id=page.Input.Id});}
            catch(AppException ex){ModelState.AddModelError("",ex.Message);}
        page.Rooms=await hotel.RoomsAsync();page.Packages=await hotel.PackagesAsync();return View(page);
    }
    [HttpPost] public async Task<IActionResult> AddExtra(Guid id,Guid extraServiceId,int quantity=1)
    {
        try {await hotel.AddExtraAsync(id,extraServiceId,quantity,CustomerId,User.Identity?.Name??"Müşteri");TempData["Success"]="Ek hizmet rezervasyona eklendi.";}
        catch(AppException ex){TempData["Error"]=ex.Message;}return RedirectToAction(nameof(Details),new{id});
    }
    [HttpPost] public async Task<IActionResult> RemoveExtra(Guid id,Guid extraId)
    {
        try {await hotel.RemoveExtraAsync(id,extraId,CustomerId,User.Identity?.Name??"Müşteri");TempData["Success"]="Ek hizmet kaldırıldı.";}
        catch(AppException ex){TempData["Error"]=ex.Message;}return RedirectToAction(nameof(Details),new{id});
    }
    [HttpPost] public async Task<IActionResult> ApplyPromotion(Guid id,string code)
    {
        try {await hotel.ApplyPromotionAsync(id,code,CustomerId,User.Identity?.Name??"Müşteri");TempData["Success"]="Kampanya indirimi uygulandı.";}
        catch(AppException ex){TempData["Error"]=ex.Message;}return RedirectToAction(nameof(Details),new{id});
    }
    [HttpPost] public async Task<IActionResult> RemovePromotion(Guid id)
    {
        try {await hotel.RemovePromotionAsync(id,CustomerId,User.Identity?.Name??"Müşteri");TempData["Success"]="Kampanya kaldırıldı.";}
        catch(AppException ex){TempData["Error"]=ex.Message;}return RedirectToAction(nameof(Details),new{id});
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
