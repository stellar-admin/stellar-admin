using System.Net;
using IdentitySimplePlayground.Data;
using Microsoft.EntityFrameworkCore;
using StellarAdmin.Dashboard.Testing;

namespace StellarAdmin.Dashboard.EntityFrameworkCore.IntegrationTests;

public partial class EfCoreResourceControllerTests
{
    [Test]
    [Arguments("")]
    [Arguments("/Create")]
    [Arguments("/Edit/1")]
    public async Task Get_WhenPolicyRequiresAuthentication_RedirectsToLogin(string path)
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();

        // Act
        using var response = await app.Client.GetAsync("/admin/test-protected" + path);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(response.Headers.Location!.ToString()).Contains("Login");
    }

    [Test]
    [Arguments("/Create")]
    [Arguments("/Edit/1")]
    [Arguments("/Delete/1")]
    public async Task Post_WhenPolicyRequiresAuthentication_RedirectsToLogin(string path)
    {
        // Arrange
        await using var app = await TestApplication.CreateAsync();
        var token = await app.GetTokenAsync("/admin/categories/Create");

        // Act
        using var response = await app.PostAsync("/admin/test-protected" + path, token);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(response.Headers.Location!.ToString()).Contains("Login");
    }
}
