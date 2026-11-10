using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using RaceDay.Contracts;

namespace RaceDay.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var client = _httpClientFactory.CreateClient("RaceDayApi");

        var response = await client.PostAsJsonAsync(
            "api/auth/login", request);

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (result == null)
        {
            ViewBag.Error = "Login failed.";
            return View();
        }

        HttpContext.Session.SetInt32("UserId", result.UserId);
        HttpContext.Session.SetString("Role", result.Role);

        return RedirectToAction("Index", "Home");
    }
}