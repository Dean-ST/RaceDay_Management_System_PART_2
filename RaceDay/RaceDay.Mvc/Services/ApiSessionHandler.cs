using System.Net;

namespace RaceDay.Mvc.Services;

public class ApiSessionHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApiSessionHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var context = _httpContextAccessor.HttpContext;

        if (context != null)
        {
            var cookie = context.Session.GetString("ApiSessionCookie");

            if (!string.IsNullOrEmpty(cookie))
            {
                request.Headers.Add("Cookie", $".AspNetCore.Session={cookie}");
            }
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (context != null &&
            response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            foreach (var value in cookies)
            {
                if (value.StartsWith(".AspNetCore.Session="))
                {
                    var cookieValue = value.Split(';')[0]
                        .Substring(".AspNetCore.Session=".Length);

                    context.Session.SetString("ApiSessionCookie", cookieValue);
                }
            }
        }

        return response;
    }
}