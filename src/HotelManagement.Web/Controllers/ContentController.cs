using HotelManagement.Application.Common;
using HotelManagement.Application.DTOs;
using HotelManagement.Application.Interfaces;
using HotelManagement.Web.Helpers;
using HotelManagement.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HotelManagement.Web.Controllers;

[Authorize(Roles=Roles.Managers)]
public class ContentController(IContentService content,IHotelClock clock):Controller
{
    public async Task<IActionResult> Index()=>View(new ContentDashboardPage {Home=await content.HomeAsync(),News=await content.NewsAsync(true)});

    [HttpGet] public async Task<IActionResult> Home()
    {
        var home=await content.HomeAsync();
        return View(new HomeContentEditorPage {Input=home.Input,CurrentHeroImageUrl=home.HeroImageUrl});
    }

    [HttpPost,RequestFormLimits(MultipartBodyLengthLimit=6291456)] public async Task<IActionResult> Home(HomeContentEditorPage page)
    {
        if(ModelState.IsValid)
            try
            {
                var image=await UploadReader.ReadAsync(page.HeroImage,"Meridian ana sayfa görseli");
                await content.SaveHomeAsync(page.Input,image,page.RemoveHeroImage);
                TempData["Success"]="Ana sayfa içeriği kaydedildi."; return RedirectToAction(nameof(Index));
            }
            catch(AppException ex){ModelState.AddModelError("",ex.Message);}
        page.CurrentHeroImageUrl=(await content.HomeAsync()).HeroImageUrl;
        return View(page);
    }

    [HttpGet] public async Task<IActionResult> EditNews(Guid? id)
    {
        if(id==null) return View(new NewsEditorPage {Input=new(){PublishedAt=clock.Today,IsPublished=true}});
        var item=(await content.NewsAsync(true)).FirstOrDefault(x=>x.Id==id);
        if(item==null)return NotFound();
        return View(new NewsEditorPage {CurrentCoverImageUrl=item.CoverImageUrl,Input=new(){Id=item.Id,Slug=item.Slug,Category=item.Category,
            Title=item.Title,Summary=item.Summary,Body=item.Body,PublishedAt=item.PublishedAt,ReadingMinutes=item.ReadingMinutes,
            IsPublished=item.IsPublished,IsFeatured=item.IsFeatured,SortOrder=item.SortOrder}});
    }

    [HttpPost,RequestFormLimits(MultipartBodyLengthLimit=6291456)] public async Task<IActionResult> EditNews(NewsEditorPage page)
    {
        if(ModelState.IsValid)
            try
            {
                var image=await UploadReader.ReadAsync(page.CoverImage,page.Input.Title+" kapak görseli");
                await content.SaveNewsAsync(page.Input,image,page.RemoveCoverImage);
                TempData["Success"]="Haber kaydedildi."; return RedirectToAction(nameof(Index));
            }
            catch(AppException ex){ModelState.AddModelError("",ex.Message);}
        if(page.Input.Id!=Guid.Empty) page.CurrentCoverImageUrl=(await content.NewsAsync(true)).FirstOrDefault(x=>x.Id==page.Input.Id)?.CoverImageUrl;
        return View(page);
    }

    [HttpPost] public async Task<IActionResult> SetNewsPublished(Guid id,bool published)
    {
        try {await content.SetNewsPublishedAsync(id,published);TempData["Success"]=published?"Haber yayınlandı.":"Haber yayından kaldırıldı.";}
        catch(AppException ex){TempData["Error"]=ex.Message;}
        return RedirectToAction(nameof(Index));
    }
}
