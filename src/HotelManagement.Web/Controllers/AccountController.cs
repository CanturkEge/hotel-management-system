using HotelManagement.Application.Common;
using HotelManagement.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
namespace HotelManagement.Web.Controllers;

public class AccountController(IAccountService accounts) : Controller
{
    [AllowAnonymous,HttpGet] public IActionResult Login()=>View(new LoginInput());
    [AllowAnonymous,HttpGet] public IActionResult StaffLogin()=>View("Login",new LoginInput{Staff=true});
    [AllowAnonymous,HttpPost,EnableRateLimiting("accounts")]
    public async Task<IActionResult> Login(LoginInput input)
    {
        if(ModelState.IsValid && await accounts.LoginAsync(input))
            return input.Staff ? RedirectToAction("Index","Staff") : RedirectToAction("Index","Bookings");
        ModelState.AddModelError("","Giriş yapılamadı. Bilgilerinizi ve müşteri/personel girişini kontrol edin. Çok sayıda denemede 15 dakika bekleyin.");
        input.Password=""; ModelState.Remove(nameof(input.Password)); return View(input);
    }
    [AllowAnonymous,HttpGet] public IActionResult Register()=>View(new RegisterInput());
    [AllowAnonymous,HttpPost,EnableRateLimiting("accounts")]
    public async Task<IActionResult> Register(RegisterInput input)
    {
        if(ModelState.IsValid)
        {
            try {await accounts.RegisterAsync(input);TempData["Success"]="Hesabınız oluşturuldu. Giriş yapabilirsiniz.";return RedirectToAction(nameof(Login));}
            catch(AppException){ModelState.AddModelError("","Kayıt yapılamadı. E-posta kullanımda olabilir veya şifre kuralları karşılanmıyor.");}
        }
        return View(input);
    }
    [Authorize,HttpPost] public async Task<IActionResult> Logout(){await accounts.LogoutAsync();return RedirectToAction("Index","Home");}
    [AllowAnonymous] public IActionResult Denied(){Response.StatusCode=403;return View();}
}
