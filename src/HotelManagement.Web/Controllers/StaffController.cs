using HotelManagement.Application.Common;
using HotelManagement.Application.Interfaces;
using HotelManagement.Application.DTOs;
using HotelManagement.Web.Models;
using HotelManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HotelManagement.Web.Controllers;

[Authorize(Roles=Roles.Staff)]
public class StaffController(IHotelService hotel,IAccountService accounts,IHotelClock clock) : Controller
{
    public async Task<IActionResult> Index()
    {
        if(User.IsInRole(Roles.Cleaner)||User.IsInRole(Roles.Maintenance)) return RedirectToAction("Index","Jobs");
        return View(await hotel.DashboardAsync());
    }
    [Authorize(Roles=Roles.Desk)]
    public async Task<IActionResult> Reservations()=>View(await hotel.BookingsAsync());
    [Authorize(Roles=Roles.Desk),HttpGet]
    public async Task<IActionResult> Create()=>View(new StaffBookingPage {Rooms=await hotel.RoomsAsync(),
        Customers=(await accounts.UsersAsync()).Where(x=>x.Role==Roles.Customer).ToList(),
        Input=new BookingInput{CheckInDate=clock.Today,CheckOutDate=clock.Today.AddDays(1)}});
    [Authorize(Roles=Roles.Desk),HttpPost]
    public async Task<IActionResult> Create(StaffBookingPage page)
    {
        page.Customers=(await accounts.UsersAsync()).Where(x=>x.Role==Roles.Customer).ToList();
        if(!page.Customers.Any(x=>x.Id==page.CustomerId))ModelState.AddModelError("","Bir müşteri hesabı seçin.");
        if(ModelState.IsValid)
            try {await hotel.BookAsync(page.CustomerId,page.Input);TempData["Success"]="Müşteri adına talep açıldı. Listeden onaylayabilirsiniz.";return RedirectToAction(nameof(Reservations));}
            catch(AppException ex){ModelState.AddModelError("",ex.Message);}
        page.Rooms=await hotel.RoomsAsync();return View(page);
    }
    [Authorize(Roles=Roles.Desk),HttpPost]
    public async Task<IActionResult> Transition(Guid id,ReservationStatus target)
    {
        try {await hotel.ChangeBookingAsync(id,target);TempData["Success"]="Rezervasyon güncellendi.";}
        catch(AppException ex){TempData["Error"]=ex.Message;}
        return RedirectToAction(nameof(Reservations));
    }
}
