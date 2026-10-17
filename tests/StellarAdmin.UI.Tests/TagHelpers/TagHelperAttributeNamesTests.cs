using System.Reflection;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace StellarAdmin.UI.Tests.TagHelpers;

public class TagHelperAttributeNamesTests
{
    [Test]
    public async Task BoundProperties_InStellarAdminUI_DeclareTheirAttributeName()
    {
        // Arrange
        var assembly = typeof(StellarAdminTagHelperBase).Assembly;

        // Act
        var unnamed = FindPropertiesWithoutAttributeName(assembly);

        // Assert
        await Assert.That(string.Join(", ", unnamed)).IsEmpty();
    }

    // Razor binds every public settable property, deriving the attribute name from the
    // property name when it isn't declared, so a C# rename would silently rename the attribute
    private static List<string> FindPropertiesWithoutAttributeName(Assembly assembly) =>
        assembly
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false, IsPublic: true }
                && typeof(ITagHelper).IsAssignableFrom(type)
            )
            .SelectMany(type =>
                type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(property =>
                        property.SetMethod?.IsPublic == true
                        && property.GetMethod?.IsPublic == true
                        && property.GetIndexParameters().Length == 0
                        && property.GetCustomAttribute<HtmlAttributeNameAttribute>() is null
                        && property.GetCustomAttribute<HtmlAttributeNotBoundAttribute>() is null
                    )
                    .Select(property => $"{property.DeclaringType!.Name}.{property.Name}")
            )
            .Distinct()
            .Order()
            .ToList();
}
