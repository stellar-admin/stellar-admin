using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StellarAdmin.Icons;

namespace StellarAdmin.TagHelpers.Tests.Support;

internal sealed class RenderingContext : IDisposable
{
    private readonly ServiceProvider _provider;

    public IHtmlGenerator Generator { get; }
    public IOptions<IconOptions> Icons { get; }
    public IModelMetadataProvider Metadata { get; }
    public ViewContext ViewContext { get; }

    public RenderingContext()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore().AddViews();
        services.AddStellarAdmin().AddTagHelpers();
        _provider = services.BuildServiceProvider();
        Generator = _provider.GetRequiredService<IHtmlGenerator>();
        Icons = _provider.GetRequiredService<IOptions<IconOptions>>();
        Metadata = _provider.GetRequiredService<IModelMetadataProvider>();
        ViewContext = new ViewContext
        {
            HttpContext = new DefaultHttpContext { RequestServices = _provider },
            ViewData = new ViewDataDictionary(Metadata, new ModelStateDictionary()),
            RouteData = new RouteData(),
            ActionDescriptor = new ActionDescriptor(),
            FormContext = new FormContext(),
        };
    }

    public void Dispose() => _provider.Dispose();
}
