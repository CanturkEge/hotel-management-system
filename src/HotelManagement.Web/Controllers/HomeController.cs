using HotelManagement.Application.Common;
using HotelManagement.Application.Interfaces;
using HotelManagement.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HotelManagement.Web.Controllers;

[AllowAnonymous]
public class HomeController(IHotelService hotel,IContentService content,IHotelClock clock) : Controller
{
    public async Task<IActionResult> Index()
    {
        var types=await hotel.TypesAsync();
        var featured=types.Where(x=>x.IsFeatured).OrderBy(x=>x.FeaturedOrder).ThenBy(x=>x.BasePrice).ToList();
        if(featured.Count==0)featured=types.Take(4).ToList();
        var news=(await content.NewsAsync()).Where(x=>x.IsFeatured).Take(3).ToList();
        return View(new CatalogPage(featured,await hotel.ReviewsAsync(),await content.HomeAsync(),news));
    }
    [HttpGet("/hakkimizda")]
    public IActionResult About()=>View();
    [HttpGet("/haberler")]
    public async Task<IActionResult> News()=>View(await content.NewsAsync());
    [HttpGet("/haberler/{slug}")]
    public async Task<IActionResult> NewsDetail(string slug)
    {
        var article=await content.NewsBySlugAsync(slug);
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
