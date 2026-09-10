using System.Security.Claims;
using HotelManagement.Application.Common;
using HotelManagement.Application.DTOs;
using HotelManagement.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace HotelManagement.Web.Controllers;

[Authorize(Roles=Roles.SuperAdmin)]
public class UsersController(IAccountService accounts):Controller
{
    public async Task<IActionResult> Index()=>View(new UsersPage{Users=await accounts.UsersAsync()});
    [HttpPost] public async Task<IActionResult> Create(UsersPage page)
    {
        if(ModelState.IsValid)
            try {await accounts.CreateStaffAsync(page.Input);TempData["Success"]="Personel hesabı oluşturuldu.";return RedirectToAction(nameof(Index));}
            catch(AppException ex){ModelState.AddModelError("",ex.Message);}
        page.Users=await accounts.UsersAsync();return View("Index",page);
    }
    [HttpPost] public async Task<IActionResult> Role(Guid id,string role)
    {
        try {await accounts.ChangeRoleAsync(id,role,Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));TempData["Success"]="Yetki değiştirildi; eski oturum sonraki istekte geçersiz olur.";}
        catch(AppException ex){TempData["Error"]=ex.Message;}
        return RedirectToAction(nameof(Index));
    }
}
