using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Editors;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

public class FieldLabelTests
{
    [Test]
    [Arguments("LeadPassenger", "Lead traveller")]
    [Arguments("DepartureDate", "Departure Date")]
    [Arguments("PassengerCount", "Passenger Count")]
    [Arguments("ReferenceCode", "Reference Code")]
    [Arguments("SupplierId", "Supplier")]
    [Arguments("Billing_Street", "Billing street")]
    [Arguments("Shipping_Street", "Shipping street")]
    public async Task CreateForm_LabelsFieldsByTitleDisplayNameResourceOrSplitName(
        string id,
        string label
    )
    {
        // Arrange
        await using var sut = await CreateHost();
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.RequiredElement($"label[for='Entity_{id}']").TextContent.Trim())
            .IsEqualTo(label);
    }

    [Test]
    public async Task InvalidSubmission_NamesEachFieldInItsMessagesAsItsLabel()
    {
        // Arrange
        await using var sut = await CreateHost();
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client);
        values["Entity.LeadPassenger"] = "";
        values["Entity.DepartureDate"] = "soon";
        values["Entity.PassengerCount"] = "12";
        values["Entity.ReferenceCode"] = "";
        values["Entity.SupplierId"] = "first";
        values["Entity.Billing.Street"] = "";
        values["Entity.Shipping.Street"] = "";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);
        var document = await response.ReadDocumentAsync();

        // Assert
        string Error(string field) =>
            document.RequiredElement($"[data-valmsg-for='Entity.{field}']").TextContent.Trim();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(Error("LeadPassenger"))
            .IsEqualTo("The Lead traveller field is required.");
        await Assert
            .That(Error("DepartureDate"))
            .IsEqualTo("The value 'soon' is not valid for Departure Date.");
        await Assert
            .That(Error("PassengerCount"))
            .IsEqualTo("The field Passenger Count must be between 1 and 9.");
        await Assert
            .That(Error("ReferenceCode"))
            .IsEqualTo("The Reference Code field is required.");
        await Assert
            .That(Error("SupplierId"))
            .IsEqualTo("The value 'first' is not valid for Supplier.");
        await Assert
            .That(Error("Billing.Street"))
            .IsEqualTo("The Billing street field is required.");
        await Assert
            .That(Error("Shipping.Street"))
            .IsEqualTo("The Shipping street field is required.");
    }

    [Test]
    public async Task DisplayField_WithoutTitle_IsLabelledBySplitName()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields.AddDisplayField(model => model.DepartureDate)
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='display-field-label']").TextContent.Trim())
            .IsEqualTo("Departure Date");
    }

    private static Task<WebApplication> CreateHost(
        Action<ResourceFieldsBuilder<FieldLabelsModel>>? configureFields = null
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<FieldLabelsModel, FieldLabelsHandler>(create =>
                    create.Fields(
                        configureFields
                            ?? (
                                fields =>
                                {
                                    fields.Add(model => model.LeadPassenger).Title =
                                        "Lead traveller";
                                    fields.Add(model => model.DepartureDate);
                                    fields.Add(model => model.PassengerCount);
                                    fields.Add(model => model.ReferenceCode);
                                    fields
                                        .Add(model => model.SupplierId)
                                        .UseSelectEditor(select => select.UseResource<Supplier>());
                                    fields.Add(model => model.Billing.Street).Title =
                                        "Billing street";
                                    fields.Add(model => model.Shipping.Street).Title =
                                        "Shipping street";
                                }
                            )
                    )
                ),
            admin =>
                admin.AddResource<Supplier>(supplier =>
                {
                    supplier.UseDataSource<SupplierDataSource>();
                    supplier.UseKey(item => item.Id);
                }),
            services => services.AddSingleton<SupplierStore>().AddSingleton<SupplierDataSource>()
        );
}
