using GolBet.Entities;
using GolBet.Repositories.Data;
using GolBet.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GolBet.Web.Controllers;

public class AccountController : Controller
{
    private const decimal WelcomeBalance = 100_000m;

    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public AccountController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            Balance = WelcomeBalance
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            AddIdentityErrors(createResult);
            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, DbSeeder.BettorRole);
        if (!roleResult.Succeeded)
        {
            AddIdentityErrors(roleResult);
            await _userManager.DeleteAsync(user);
            return View(model);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        TempData["Success"] = $"¡Bienvenido a GolBet, {user.FullName}! Tienes 100.000 FutCoins para empezar.";
        return RedirectToAction("Index", "Matches");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var result = await _signInManager.PasswordSignInAsync(
            model.Email,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(model);
        }

        return RedirectToAction("Index", "Matches");
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied() => View();

    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
