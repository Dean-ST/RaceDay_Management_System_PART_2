using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using RaceDay.Contracts;

namespace RaceDay.Mvc.Controllers;

public class EnrolmentsController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EnrolmentsController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Mine()
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        var enrolments = await client.GetFromJsonAsync<List<EnrolmentDto>>(
            "api/enrolments/mine");

        return View(enrolments ?? new List<EnrolmentDto>());
    }

    public async Task<IActionResult> Event(int eventId)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        var enrolments = await client.GetFromJsonAsync<List<EnrolmentDto>>(
            $"api/enrolments/event/{eventId}");

        ViewBag.EventId = eventId;

        return View(enrolments ?? new List<EnrolmentDto>());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEnrolmentRequest request)
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        var response = await client.PostAsJsonAsync(
            "api/enrolments", request);

        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Could not enrol. Please check your selection.";
        }

        return RedirectToAction("Mine");
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(
        int id, int eventId, string status)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Account");

        var client = _httpClientFactory.CreateClient("RaceDayApi");

        await client.PutAsJsonAsync(
            $"api/enrolments/{id}/status",
            new UpdateEnrolmentStatusRequest(status));

        return RedirectToAction("Event", new { eventId });
    }
}