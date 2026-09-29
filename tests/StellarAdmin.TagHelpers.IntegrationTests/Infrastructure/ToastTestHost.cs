using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace StellarAdmin.TagHelpers.IntegrationTests.Infrastructure;

internal static class ToastTestHost
{
    public static async Task<WebApplication> CreateAsync()
    {
        var builder = WebApplication.CreateBuilder(
            new WebApplicationOptions
            {
                ApplicationName = typeof(ToastTestHost).Assembly.GetName().Name,
                EnvironmentName = "Development",
            }
        );
        builder.WebHost.UseTestServer();
        builder.Services.AddControllers();
        builder.Services.AddRazorPages();
        builder.Services.AddStellarAdmin().AddTagHelpers();

        var app = builder.Build();
        app.MapControllers();
        app.MapRazorPages();
        await app.StartAsync();

        return app;
    }

    // TestServer's client ignores cookies, and TempData travels in a cookie between requests.
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
