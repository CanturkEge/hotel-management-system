using HotelManagement.Application.Common;
using HotelManagement.Application.Interfaces;
using HotelManagement.Domain.Enums;
using HotelManagement.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HotelManagement.Web.Controllers;

[Authorize(Roles=Roles.Staff)]
public class JobsController(IHotelService hotel):Controller
{
    private JobKind? AllowedKind=>User.IsInRole(Roles.Cleaner)?JobKind.Cleaning:User.IsInRole(Roles.Maintenance)?JobKind.Maintenance:null;
    public async Task<IActionResult> Index()=>View(new JobPage{Rooms=await hotel.RoomsAsync(),Jobs=await hotel.JobsAsync(AllowedKind)});
    [HttpPost] public async Task<IActionResult> Report(JobPage page)
    {
        if(ModelState.IsValid)
            try {await hotel.ReportJobAsync(page.Input);TempData["Success"]="Görev açıldı; oda yeni rezervasyonlara kapatıldı.";return RedirectToAction(nameof(Index));}
            catch(AppException ex){ModelState.AddModelError("",ex.Message);}
        page.Rooms=await hotel.RoomsAsync();page.Jobs=await hotel.JobsAsync(AllowedKind);return View("Index",page);
    }
    [HttpPost] public async Task<IActionResult> Complete(Guid id)
    {
        // Reception can report incidents, but cannot mark cleaning or repair completed.
        if(User.IsInRole(Roles.Reception))return Forbid();
        try {await hotel.CompleteJobAsync(id,AllowedKind,User.Identity?.Name??"Personel");TempData["Success"]="Görev tamamlandı.";}
        catch(AppException ex){TempData["Error"]=ex.Message;}
        return RedirectToAction(nameof(Index));
    }
}
