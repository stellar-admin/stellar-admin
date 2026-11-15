using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;

namespace StellarAdmin.Testing;

public static class TestClientExtensions
{
    // TestServer's client ignores cookies, and the antiforgery token and TempData travel in cookies between requests
    public static HttpClient CreateClient(this WebApplication app)
    {
        var handler = new CookieHandler { InnerHandler = app.GetTestServer().CreateHandler() };

        return new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
    }

    private sealed class CookieHandler : DelegatingHandler
    {
        private readonly CookieContainer _cookies = new();

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            var uri = request.RequestUri!;
            var cookieHeader = _cookies.GetCookieHeader(uri);
            if (cookieHeader.Length > 0)
            {
                request.Headers.Add("Cookie", cookieHeader);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
            {
                foreach (var setCookie in setCookies)
                {
                    _cookies.SetCookies(uri, setCookie);
                }
            }

            return response;
        }
    }
}
