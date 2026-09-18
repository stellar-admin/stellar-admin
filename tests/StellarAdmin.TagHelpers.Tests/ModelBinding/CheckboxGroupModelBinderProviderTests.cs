using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using StellarAdmin.TagHelpers.Tests.Support;

namespace StellarAdmin.TagHelpers.Tests.ModelBinding;

public class CheckboxGroupModelBinderProviderTests
{
    [Test]
    public async Task BindModelAsync_MarkerOnly_ClearsExistingNestedCollectionAndRecordsEmptyState()
    {
        // Arrange
        using var context = new RenderingContext();
        var existing = new InputModel { Roles = [2, 3] };
        var binding = await CreateBindingAsync(
            context,
            typeof(InputModel),
            existing,
            new() { ["__sa_checkbox_group.Input.Roles"] = "true" }
        );
        var sut = binding.Binder;

        // Act
        await sut.BindModelAsync(binding.Context);

        // Assert
        await Assert.That(existing.Roles.Length).IsEqualTo(0);
        await Assert
            .That(binding.Context.ModelState["Input.Roles"]?.RawValue as string[])
            .IsEmpty();
        await Assert.That(binding.Context.ModelState.ErrorCount).IsEqualTo(0);
    }

    [Test]
    public async Task BindModelAsync_OmittedGroup_PreservesExistingCollection()
    {
        // Arrange
        using var context = new RenderingContext();
        var existing = new InputModel { Roles = [2, 3] };
        var binding = await CreateBindingAsync(
            context,
            typeof(InputModel),
            existing,
            new() { ["Input.Title"] = "Updated" }
        );
        var sut = binding.Binder;

        // Act
        await sut.BindModelAsync(binding.Context);

        // Assert
        await Assert.That(existing.Roles).IsEquivalentTo(new[] { 2, 3 });
        await Assert.That(existing.Title).IsEqualTo("Updated");
    }

    [Test]
    public async Task BindModelAsync_RepeatedValues_BindsTypedCollection()
    {
        // Arrange
        using var context = new RenderingContext();
        var existing = new InputModel { Roles = [2, 3] };
        var binding = await CreateBindingAsync(
            context,
            typeof(InputModel),
            existing,
            new()
            {
                ["Input.Roles"] = new StringValues(["1", "4"]),
                ["__sa_checkbox_group.Input.Roles"] = "true",
            }
        );
        var sut = binding.Binder;

        // Act
        await sut.BindModelAsync(binding.Context);

        // Assert
        await Assert.That(existing.Roles).IsEquivalentTo(new[] { 1, 4 });
        await Assert
            .That(binding.Context.ModelState["Input.Roles"]?.RawValue as string[])
            .IsEquivalentTo(new[] { "1", "4" });
    }

    [Test]
    public async Task BindModelAsync_InvalidValue_PreservesAttemptedValueAndConversionError()
    {
        // Arrange
        using var context = new RenderingContext();
        var binding = await CreateBindingAsync(
            context,
            typeof(InputModel),
            new InputModel(),
            new() { ["Input.Roles"] = "bad", ["__sa_checkbox_group.Input.Roles"] = "true" }
        );
        var sut = binding.Binder;

        // Act
        await sut.BindModelAsync(binding.Context);

        // Assert
        await Assert
            .That(binding.Context.ModelState["Input.Roles"]?.AttemptedValue)
            .IsEqualTo("bad");
        await Assert.That(binding.Context.ModelState["Input.Roles"]?.Errors.Count).IsEqualTo(1);
    }

    [Test]
    [Arguments(typeof(string[]), "hello")]
    [Arguments(typeof(Guid[]), "b58e6b78-fb5a-4a82-b94d-9b951146528f")]
    [Arguments(typeof(bool[]), "true")]
    [Arguments(typeof(DayOfWeek[]), "Monday")]
    [Arguments(typeof(List<int>), "3")]
    [Arguments(typeof(IList<int>), "3")]
    [Arguments(typeof(ICollection<int>), "3")]
    [Arguments(typeof(IEnumerable<int>), "3")]
    [Arguments(typeof(IReadOnlyList<int>), "3")]
    [Arguments(typeof(IReadOnlyCollection<int>), "3")]
    public async Task BindModelAsync_SupportedCollection_BindsValue(Type type, string value)
    {
        // Arrange
        using var context = new RenderingContext();
        var binding = await CreateBindingAsync(
            context,
            type,
            null,
            new() { ["Input"] = value, ["__sa_checkbox_group.Input"] = "true" }
        );
        var sut = binding.Binder;

        // Act
        await sut.BindModelAsync(binding.Context);

        // Assert
        await Assert.That(binding.Context.Result.IsModelSet).IsTrue();
        await Assert.That(binding.Context.ModelState.ErrorCount).IsEqualTo(0);
        await Assert
            .That(
                ((System.Collections.IEnumerable)binding.Context.Result.Model!)
                    .Cast<object>()
                    .Count()
            )
            .IsEqualTo(1);
    }

    [Test]
    public async Task BindModelAsync_LocalizedDecimal_UsesFormCulture()
    {
        // Arrange
        using var context = new RenderingContext();
        var previousCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
        try
        {
            var binding = await CreateBindingAsync(
                context,
                typeof(decimal[]),
                null,
                new() { ["Input"] = "1,25" }
            );
            var sut = binding.Binder;

            // Act
            await sut.BindModelAsync(binding.Context);

            // Assert
            await Assert
                .That(binding.Context.Result.Model as decimal[])
                .IsEquivalentTo(new[] { 1.25m });
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
        }
    }

    [Test]
    [Arguments(typeof(List<int>))]
    [Arguments(typeof(IList<int>))]
    [Arguments(typeof(ICollection<int>))]
    [Arguments(typeof(IEnumerable<int>))]
    [Arguments(typeof(IReadOnlyList<int>))]
    [Arguments(typeof(IReadOnlyCollection<int>))]
    public async Task BindModelAsync_MarkerForList_BindsEmptyCollection(Type type)
    {
        // Arrange
        using var context = new RenderingContext();
        var binding = await CreateBindingAsync(
            context,
            type,
            null,
            new() { ["__sa_checkbox_group.Input"] = "true" }
        );
        var sut = binding.Binder;

        // Act
        await sut.BindModelAsync(binding.Context);

        // Assert
        await Assert.That(binding.Context.Result.IsModelSet).IsTrue();
        await Assert
            .That(((System.Collections.IEnumerable)binding.Context.Result.Model!).Cast<object>())
            .IsEmpty();
        await Assert.That(binding.Context.ModelState["Input"]?.RawValue as string[]).IsEmpty();
    }

    [Test]
    public async Task BindModelAsync_QueryBoundProperty_DoesNotClearFromFormMarker()
    {
        // Arrange
        using var context = new RenderingContext();
        var existing = new QueryModel { Roles = [2, 3] };
        var binding = await CreateBindingAsync(
            context,
            typeof(QueryModel),
            existing,
            new() { ["__sa_checkbox_group.Input.Roles"] = "true" }
        );
        var sut = binding.Binder;

        // Act
        await sut.BindModelAsync(binding.Context);

        // Assert
        await Assert.That(existing.Roles).IsEquivalentTo(new[] { 2, 3 });
    }

    private static async Task<(
        IModelBinder Binder,
        ModelBindingContext Context
    )> CreateBindingAsync(
        RenderingContext context,
        Type modelType,
        object? model,
        Dictionary<string, StringValues> form
    )
    {
        context.ViewContext.HttpContext.Request.ContentType = "application/x-www-form-urlencoded";
        context.ViewContext.HttpContext.Request.Form = new FormCollection(form);
        var services = context.ViewContext.HttpContext.RequestServices;
        var metadata = context.Metadata.GetMetadataForType(modelType);
        var valueProvider = await CompositeValueProvider.CreateAsync(
            context.ViewContext,
            services.GetRequiredService<IOptions<MvcOptions>>().Value.ValueProviderFactories
        );
        var bindingContext = DefaultModelBindingContext.CreateBindingContext(
            context.ViewContext,
            valueProvider,
            metadata,
            null,
            "Input"
        );
        bindingContext.Model = model;
        var binder = services
            .GetRequiredService<IModelBinderFactory>()
            .CreateBinder(
                new ModelBinderFactoryContext { Metadata = metadata, CacheToken = metadata }
            );
        return (binder, bindingContext);
    }

    public class QueryModel
    {
        [FromQuery]
        public int[] Roles { get; set; } = [];
    }

    public class InputModel
    {
        public int[] Roles { get; set; } = [];
        public string? Title { get; set; }
    }
}
