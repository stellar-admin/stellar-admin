using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using StellarAdmin.Dashboard.IntegrationTests.Fixtures;
using StellarAdmin.Dashboard.IntegrationTests.Infrastructure;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Editors;
using static StellarAdmin.Dashboard.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.Dashboard.IntegrationTests.Resources;

public class TextInputEditorTests
{
    [Test]
    [Arguments("Budget", "number", "any")]
    [Arguments("Code", "text", null)]
    [Arguments("Email", "email", null)]
    [Arguments("ExternalId", "text", null)]
    [Arguments("Phone", "tel", null)]
    [Arguments("Population", "number", "1")]
    [Arguments("Quantity", "number", "1")]
    [Arguments("Ratio", "number", "any")]
    [Arguments("Website", "url", null)]
    public async Task DataTypeTemplate_InfersInputTypeAndStep(
        string property,
        string expectedType,
        string? expectedStep
    )
    {
        // Arrange
        await using var sut = await CreateTextInputFieldsHost(fields =>
        {
            fields.Add(model => model.Budget);
            fields.Add(model => model.Code);
            fields.Add(model => model.Email);
            fields.Add(model => model.ExternalId);
            fields.Add(model => model.Phone);
            fields.Add(model => model.Population);
            fields.Add(model => model.Quantity);
            fields.Add(model => model.Ratio);
            fields.Add(model => model.Website);
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var input = document.RequiredElement($"input[name='Entity.{property}']");
        await Assert.That(input.GetAttribute("type")).IsEqualTo(expectedType);
        await Assert.That(input.GetAttribute("step")).IsEqualTo(expectedStep);
        await Assert.That(input.HasAttribute("readonly")).IsFalse();
        await Assert.That(input.HasAttribute("placeholder")).IsFalse();
    }

    [Test]
    public async Task DataTypeTemplate_PreservesFieldMetadata()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var name = document.RequiredElement("input[name='Entity.Name']");
        await Assert.That(name.GetAttribute("id")).IsEqualTo("Entity_Name");
        await Assert.That(name.GetAttribute("data-val-required")).IsNotNull();
        await Assert
            .That(document.RequiredElement("label[for='Entity_Name']").TextContent.Trim())
            .IsEqualTo("Product name");
        var price = document.RequiredElement("input[name='Entity.Price']");
        await Assert.That(price.GetAttribute("type")).IsEqualTo("number");
        await Assert.That(price.GetAttribute("data-val-range")).IsNotNull();
    }

    [Test]
    public async Task DataTypeTemplate_InvalidSubmission_ShowsFieldErrors()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(new([]));
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "";
        values["Entity.Price"] = "0";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(document.RequiredElement("[data-valmsg-for='Entity.Name']").TextContent.Trim())
            .IsNotEmpty();
        await Assert
            .That(document.RequiredElement("[data-valmsg-for='Entity.Price']").TextContent.Trim())
            .IsNotEmpty();
        await Assert
            .That(document.RequiredElement("input[name='Entity.Price']").GetAttribute("value"))
            .IsEqualTo("0");
    }

    [Test]
    public async Task FieldDescription_OverridesAndAddsHelpText()
    {
        // Arrange
        await using var sut = await CreateTextInputFieldsHost(fields =>
        {
            fields.Add(model => model.Code).Description = "Printed on the boarding pass.";
            fields.Add(model => model.Email).Description = "We send the itinerary here.";
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var code = document
            .RequiredElement("input[name='Entity.Code']")
            .Closest("[data-slot='field']")!;
        await Assert
            .That(code.RequiredElement("[data-slot='field-description']").TextContent.Trim())
            .IsEqualTo("Printed on the boarding pass.");
        var email = document
            .RequiredElement("input[name='Entity.Email']")
            .Closest("[data-slot='field']")!;
        await Assert
            .That(email.RequiredElement("[data-slot='field-description']").TextContent.Trim())
            .IsEqualTo("We send the itinerary here.");
    }

    [Test]
    public async Task FieldDescription_WithPrefix_DescribesInput()
    {
        // Arrange
        await using var sut = await CreateTextInputFieldsHost(fields =>
            fields
                .Add(model => model.Email)
                .UseEditor<TextInputEditor>(input => input.Prefix = "@")
                .Description = "We send the itinerary here."
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var input = document.RequiredElement("input[name='Entity.Email']");
        await Assert
            .That(input.GetAttribute("aria-describedby"))
            .IsEqualTo("Entity_Email-description Entity_Email-error");
        await Assert
            .That(document.RequiredElement("#Entity_Email-description").TextContent.Trim())
            .IsEqualTo("We send the itinerary here.");
    }

    [Test]
    public async Task UseEditor_PasswordProperty_InfersPasswordType()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<CreateProductModel, CreateProductHandler>(create =>
                {
                    create.UseFactory(() => new(10m));
                    create.Fields(fields =>
                    {
                        fields.Add(model => model.ProductName).UseEditor<TextInputEditor>();
                        fields.Add(model => model.Password).UseEditor<TextInputEditor>();
                    });
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var name = document.RequiredElement("input[name='Entity.ProductName']");
        await Assert.That(name.GetAttribute("type")).IsEqualTo("text");
        await Assert.That(name.GetAttribute("data-val-required")).IsNotNull();
        var password = document.RequiredElement("input[name='Entity.Password']");
        await Assert.That(password.GetAttribute("type")).IsEqualTo("password");
    }

    [Test]
    public async Task UseEditor_ExplicitSettings_OverrideInference()
    {
        // Arrange
        await using var sut = await CreateTextInputFieldsHost(fields =>
        {
            fields
                .Add(model => model.Budget)
                .UseEditor<TextInputEditor>(input =>
                {
                    input.Max = 500m;
                    input.Min = 0m;
                    input.Placeholder = "0.00";
                    input.Step = 0.01m;
                });
            fields
                .Add(model => model.Quantity)
                .UseEditor<TextInputEditor>(input => input.Type = TextInputType.Text);
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var budget = document.RequiredElement("input[name='Entity.Budget']");
        await Assert.That(budget.GetAttribute("type")).IsEqualTo("number");
        await Assert.That(budget.GetAttribute("step")).IsEqualTo("0.01");
        await Assert.That(budget.GetAttribute("min")).IsEqualTo("0");
        await Assert.That(budget.GetAttribute("max")).IsEqualTo("500");
        await Assert.That(budget.GetAttribute("placeholder")).IsEqualTo("0.00");
        var quantity = document.RequiredElement("input[name='Entity.Quantity']");
        await Assert.That(quantity.GetAttribute("type")).IsEqualTo("text");
        await Assert.That(quantity.HasAttribute("step")).IsFalse();
    }

    [Test]
    public async Task UseEditor_PrefixAndSuffix_RendersInputGroupWithFieldParts()
    {
        // Arrange
        await using var sut = await CreateTextInputFieldsHost(fields =>
            fields
                .Add(model => model.Code)
                .UseEditor<TextInputEditor>(input =>
                {
                    input.ClassNames.Control = "code-group";
                    input.Prefix = "INV-";
                    input.Suffix = "/2026";
                })
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var input = document.RequiredElement("input[name='Entity.Code']");
        var group = input.Closest("[data-slot='input-group']")!;
        await Assert.That(group.ClassList.Contains("code-group")).IsTrue();
        var addOns = group.QuerySelectorAll("[data-slot='input-group-addon']");
        await Assert
            .That(addOns.Select(addOn => addOn.TextContent.Trim()))
            .IsEquivalentTo(["INV-", "/2026"]);
        await Assert.That(input.HasAttribute("readonly")).IsFalse();
        await Assert
            .That(document.RequiredElement("label[for='Entity_Code']").TextContent.Trim())
            .IsEqualTo("Code");
        await Assert
            .That(input.GetAttribute("aria-describedby"))
            .IsEqualTo("Entity_Code-description Entity_Code-error");
        await Assert
            .That(document.RequiredElement("#Entity_Code-description").TextContent.Trim())
            .IsEqualTo("Shown on the invoice.");
        await Assert
            .That(document.RequiredElement("#Entity_Code-error").GetAttribute("data-valmsg-for"))
            .IsEqualTo("Entity.Code");
    }

    [Test]
    public async Task UseEditor_PrefixWithInvalidSubmission_MarksInputInvalid()
    {
        // Arrange
        await using var sut = await DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                    {
                        fields.Add(product => product.Name);
                        fields
                            .Add(product => product.Price)
                            .UseEditor<TextInputEditor>(input => input.Prefix = "$");
                    })
                )
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "Lamp";
        values["Entity.Price"] = "0";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        var price = document.RequiredElement("input[name='Entity.Price']");
        await Assert.That(price.GetAttribute("aria-invalid")).IsEqualTo("true");
        await Assert.That(price.GetAttribute("data-val-range")).IsNotNull();
        await Assert
            .That(document.RequiredElement("#Entity_Price-error").TextContent.Trim())
            .IsNotEmpty();
    }

    private static Task<WebApplication> CreateTextInputFieldsHost(
        Action<ResourceFieldsBuilder<TextInputFieldsModel>> configureFields
    ) =>
        DashboardTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<TextInputFieldsModel, TextInputFieldsHandler>(create =>
                {
                    create.UseFactory(() => new());
                    create.Fields(configureFields);
                })
        );
}
