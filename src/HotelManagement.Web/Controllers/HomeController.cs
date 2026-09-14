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
    [HttpGet("/hakkimizda")]
    public IActionResult About()=>View();
    [HttpGet("/haberler")]
    public IActionResult News()=>View(SiteContent.News);
    [HttpGet("/haberler/{slug}")]
    public IActionResult NewsDetail(string slug)
    {
        var article=SiteContent.News.FirstOrDefault(x=>x.Slug.Equals(slug,StringComparison.OrdinalIgnoreCase));
        return article==null?NotFound():View(article);
    }
    [HttpGet("/iletisim")]
    public IActionResult Contact()=>View(new ContactInput());
    [HttpPost("/iletisim")]
    public IActionResult Contact(ContactInput input)
    {
        if(!ModelState.IsValid)return View(input);
        TempData["Success"]="Form başarıyla doğrulandı. Bu eğitim sürümünde harici e-posta servisi bağlı değildir; doğrudan e-posta veya telefon bağlantısını kullanabilirsiniz.";
        return RedirectToAction(nameof(Contact));
    }
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
