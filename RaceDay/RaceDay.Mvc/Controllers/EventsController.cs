using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using RaceDay.Contracts;

namespace RaceDay.Mvc.Controllers;

public class EventsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EventsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        var events = await client.GetFromJsonAsync<List<EventDto>>(
            "api/events");

        return View(events ?? new List<EventDto>());
    }

    public async Task<IActionResult> Browse()
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        var events = await client.GetFromJsonAsync<List<EventDto>>(
            "api/events");

        return View(events ?? new List<EventDto>());
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Account");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEventRequest request)
    {
        var client = _httpClientFactory.CreateClient("RaceDayApi");

        var response = await client.PostAsJsonAsync(
            "api/events", request);

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Could not create event.";
            return View();
        }

        return RedirectToAction("Index");
    }


    [HttpGet]
    public async Task<IActionResult> SelectCategory(int eventId)
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        var categories = await client.GetFromJsonAsync<List<CategoryDto>>(
            $"api/events/{eventId}/categories");

        ViewBag.EventId = eventId;

        return View(categories ?? new List<CategoryDto>());
    }
}