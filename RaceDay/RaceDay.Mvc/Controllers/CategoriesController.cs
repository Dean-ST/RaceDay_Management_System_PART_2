using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using RaceDay.Contracts;

namespace RaceDay.Mvc.Controllers;

public class CategoriesController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CategoriesController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private bool IsOrganiser()
    {
        return HttpContext.Session.GetString("Role") == "Organiser";
    }

    public async Task<IActionResult> Index(int eventId)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        var categories = await client.GetFromJsonAsync<List<CategoryDto>>(
            $"api/events/{eventId}/categories");

        ViewBag.EventId = eventId;

        return View(categories ?? new List<CategoryDto>());
    }

    [HttpPost]
    public async Task<IActionResult> Create(int eventId, CreateCategoryRequest request)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        await client.PostAsJsonAsync(
            $"api/events/{eventId}/categories", request);

        return RedirectToAction("Index", new { eventId });
    }

    [HttpPost]
    public async Task<IActionResult> Update(int id, int eventId, CreateCategoryRequest request)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        await client.PutAsJsonAsync(
            $"api/categories/{id}", request);

        return RedirectToAction("Index", new { eventId });
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, int eventId)
    {
        if (!IsOrganiser())
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        await client.DeleteAsync($"api/categories/{id}");

        return RedirectToAction("Index", new { eventId });
    }
}