using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using RaceDay.Contracts;

namespace RaceDay.Mvc.Controllers;

public class ResultsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ResultsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Mine()
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        var results = await client.GetFromJsonAsync<List<ResultDto>>(
            "api/results/mine");

        return View(results ?? new List<ResultDto>());
    }

    public async Task<IActionResult> Event(int eventId)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        var results = await client.GetFromJsonAsync<List<ResultDto>>(
            $"api/results/event/{eventId}");

        ViewBag.EventId = eventId;

        return View(results ?? new List<ResultDto>());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateResultRequest request, int eventId)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        var response = await client.PostAsJsonAsync(
            "api/results", request);

        if (!response.IsSuccessStatusCode)
            TempData["Error"] = "Could not save the result.";

        return RedirectToAction("Event", new { eventId });
    }
}