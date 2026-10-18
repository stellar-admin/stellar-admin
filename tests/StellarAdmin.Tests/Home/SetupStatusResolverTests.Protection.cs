using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.Home;

namespace StellarAdmin.Tests.Home;

public partial class SetupStatusResolverTests
{
    [Test]
    public async Task ResolveAsync_WithAuthorizeDataOnEndpoint_ReportsProtected()
    {
        // Arrange
        var services = CreateServices();
        services.AddStellarAdmin();
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();
        var httpContext = CreateHttpContext(new AuthorizeAttribute { Roles = "Staff" });

        // Act
        var status = await sut.ResolveAsync(httpContext);

        // Assert
        await Assert.That(status!.IsProtected).IsTrue();
    }

    [Test]
    public async Task ResolveAsync_WithPolicyOnEndpoint_ReportsProtected()
    {
        // Arrange
        var services = CreateServices();
        services.AddStellarAdmin();
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();
        var httpContext = CreateHttpContext(
            new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build()
        );

        // Act
        var status = await sut.ResolveAsync(httpContext);

        // Assert
        await Assert.That(status!.IsProtected).IsTrue();
    }

    [Test]
    public async Task ResolveAsync_WithAllowAnonymousOnEndpoint_ReportsUnprotected()
    {
        // Arrange
        var services = CreateServices();
        services.AddAuthorization(options =>
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build()
        );
        services.AddStellarAdmin();
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();
        var httpContext = CreateHttpContext(
            new AuthorizeAttribute(),
            new AllowAnonymousAttribute()
        );

        // Act
        var status = await sut.ResolveAsync(httpContext);

        // Assert
        await Assert.That(status!.IsProtected).IsFalse();
    }

    [Test]
    public async Task ResolveAsync_WithFallbackPolicy_ReportsProtected()
    {
        // Arrange
        var services = CreateServices();
        services.AddAuthorization(options =>
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build()
        );
        services.AddStellarAdmin();
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();

        // Act
        var status = await sut.ResolveAsync(CreateHttpContext());

        // Assert
        await Assert.That(status!.IsProtected).IsTrue();
    }

    [Test]
    public async Task ResolveAsync_WithGlobalAuthorizeFilter_ReportsProtected()
    {
        // Arrange
        var services = CreateServices();
        services.Configure<MvcOptions>(options => options.Filters.Add(new AuthorizeFilter()));
        services.AddStellarAdmin();
        using var provider = services.BuildServiceProvider();
        var sut = provider.GetRequiredService<SetupStatusResolver>();

        // Act
        var status = await sut.ResolveAsync(CreateHttpContext());

        // Assert
        await Assert.That(status!.IsProtected).IsTrue();
    }
}
