using HotelManagement.Application.Common;
using HotelManagement.Application.Interfaces;
using HotelManagement.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HotelManagement.Web.Controllers;

[AllowAnonymous]
public class HomeController(IHotelService hotel,IHotelClock clock) : Controller
{
    public async Task<IActionResult> Index()=>View(new CatalogPage(await hotel.TypesAsync(),await hotel.ReviewsAsync()));
    [HttpGet]
    public async Task<IActionResult> Search(DateOnly? start,DateOnly? end,int guests=2)
    {
        var page=new SearchPage{Start=start??clock.Today,End=end??clock.Today.AddDays(1),Guests=guests,Searched=start!=null};
        if(page.Searched)
        {
            if(!ModelState.IsValid) ModelState.AddModelError("","Tarih ve misafir sayısını kontrol edin.");
            else try {page.Rooms=await hotel.AvailableAsync(page.Start,page.End,page.Guests);} catch(AppException ex){ModelState.AddModelError("",ex.Message);}
        }
        return View(page);
    }
    public async Task<IActionResult> Details(Guid id)
    {
        var type=(await hotel.TypesAsync()).FirstOrDefault(x=>x.Id==id);
        if(type==null) return NotFound();
        return View(new DetailPage(type,(await hotel.RoomsAsync()).Where(x=>x.RoomTypeId==id).ToList(),(await hotel.ReviewsAsync()).Where(x=>x.RoomTypeId==id).ToList()));
    }
    public IActionResult Error()=>View();
}
