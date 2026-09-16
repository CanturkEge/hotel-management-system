using HotelManagement.Application.Common;
using HotelManagement.Application.DTOs;
using HotelManagement.Application.Interfaces;
using HotelManagement.Web.Models;
using HotelManagement.Web.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HotelManagement.Web.Controllers;

[Authorize(Roles=Roles.Managers)]
public class ManagementController(IHotelService hotel,IContentService content):Controller
{
    public async Task<IActionResult> Types()=>View(await hotel.TypesAsync(true));
    [HttpGet] public async Task<IActionResult> EditType(Guid? id)
    {
        if(id==null) return View(new RoomTypeEditorPage());
        var t=(await hotel.TypesAsync(true)).FirstOrDefault(t=>t.Id==id);
        if(t==null)return NotFound();
        return View(new RoomTypeEditorPage {Input=new() {Id=t.Id,Name=t.Name,Description=t.Description,BasePrice=t.BasePrice,Capacity=t.Capacity,
            BedCount=t.BedCount,SizeInSquareMeters=t.SizeInSquareMeters,Amenities=t.Amenities,IsActive=t.IsActive,
            IsFeatured=t.IsFeatured,FeaturedOrder=t.FeaturedOrder},ExistingImages=t.Images.ToList()});
    }
    [HttpPost,RequestFormLimits(MultipartBodyLengthLimit=41943040)] public async Task<IActionResult> EditType(RoomTypeEditorPage page)
    {
        if(ModelState.IsValid)
            try
            {
                var uploads=await UploadReader.ReadManyAsync(page.Images,page.Input.Name+" oda görseli");
                var id=await hotel.SaveTypeAsync(page.Input);
                await content.UpdateRoomImagesAsync(id,uploads,page.RemoveImageIds);
                TempData["Success"]="Oda tipi ve görseller kaydedildi.";return RedirectToAction(nameof(Types));
            }
            catch(AppException ex){ModelState.AddModelError("",ex.Message);}
        if(page.Input.Id!=Guid.Empty)
            page.ExistingImages=(await hotel.TypesAsync(true)).FirstOrDefault(x=>x.Id==page.Input.Id)?.Images.ToList()??[];
        return View(page);
    }
    public async Task<IActionResult> Rooms()=>View(await RoomPage());
    [HttpPost] public async Task<IActionResult> CreateRooms([Bind(Prefix="Bulk")] BulkRoomInput input)
    {
        if(ModelState.IsValid)
            try
            {
                var count=await hotel.CreateRoomsAsync(input);
                TempData["Success"]=$"{count} oda tek seferde oluşturuldu.";
                return RedirectToAction(nameof(Rooms));
            }
            catch(AppException ex){ModelState.AddModelError("",ex.Message);}
        return View(nameof(Rooms),await RoomPage(input));
    }
    private async Task<RoomManagementPage> RoomPage(BulkRoomInput? input=null)=>new()
    {
        Bulk=input??new(),
        Rooms=await hotel.RoomsAsync(true),
        Types=await hotel.TypesAsync()
    };
    [HttpGet] public async Task<IActionResult> EditRoom(Guid? id)
    {
        var page=new RoomEditPage{Types=await hotel.TypesAsync()};
        if(id!=null)
        {
            var r=(await hotel.RoomsAsync(true)).FirstOrDefault(r=>r.Id==id);
            if(r==null)return NotFound();
            page.Input=new RoomInput{Id=r.Id,Number=r.Number,Floor=r.Floor,RoomTypeId=r.RoomTypeId,IsActive=r.IsActive};
        }
        return View(page);
    }
    [HttpPost] public async Task<IActionResult> EditRoom(RoomEditPage page)
    {
        if(ModelState.IsValid)
            try {await hotel.SaveRoomAsync(page.Input);TempData["Success"]="Oda kaydedildi.";return RedirectToAction(nameof(Rooms));}
            catch(AppException ex){ModelState.AddModelError("",ex.Message);}
        page.Types=await hotel.TypesAsync();return View(page);
    }
    [HttpPost] public async Task<IActionResult> ArchiveRoom(Guid id)
    {
        try {await hotel.ArchiveRoomAsync(id);TempData["Success"]="Oda satıştan kaldırıldı. Geçmiş kayıtlar korundu.";}
        catch(AppException ex){TempData["Error"]=ex.Message;}
        return RedirectToAction(nameof(Rooms));
    }
    public async Task<IActionResult> Reviews()=>View(await hotel.ReviewsAsync(true));
    [HttpPost] public async Task<IActionResult> Moderate(Guid id,bool approve)
    {
        try {await hotel.ModerateAsync(id,approve);TempData["Success"]="Yorumun yayın durumu güncellendi.";}
        catch(AppException ex){TempData["Error"]=ex.Message;}
        return RedirectToAction(nameof(Reviews));
    }
}
