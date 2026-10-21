using System.Net;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourceHandlerFailureTests
{
    [Test]
    public async Task Delete_HandlerThrows_RedisplaysIndexWithErrorAndLogsException()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)])
        {
            WriteException = new InvalidOperationException("Storage is offline."),
        };
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            resource => resource.UseKey(product => product.Id).AllowDelete()
        );
        using var client = sut.GetTestClient();
        using var content = new FormUrlEncodedContent(
            await PrepareForm(client, "/stellaradmin/products")
        );

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/delete/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(document.RequiredElement(".validation-summary-errors").TextContent)
            .Contains("This record couldn't be deleted. Try again.");
        await Assert
            .That(document.RequiredElement(".validation-summary-errors").TextContent)
            .DoesNotContain("Storage is offline.");
        await Assert.That(state.Products.Count).IsEqualTo(1);
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>().Logs;
        await Assert
            .That(
                logs.Any(log =>
                    log.Level == LogLevel.Error && log.Message.Contains("The delete of a Product")
                )
            )
            .IsTrue();
    }

    [Test]
    public async Task Create_HandlerThrows_RedisplaysFormWithError()
    {
        // Arrange
        var state = new ProductState([])
        {
            WriteException = new InvalidOperationException("Storage is offline."),
        };
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields => fields.Add(product => product.Name))
                )
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/create");
        values["Entity.Name"] = "Notebook";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/create", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(document.RequiredElement(".validation-summary-errors").TextContent)
            .Contains("This record couldn't be saved. Try again.");
        await Assert
            .That(document.RequiredElement("input[name='Entity.Name']").GetAttribute("value"))
            .IsEqualTo("Notebook");
    }

    [Test]
    public async Task Edit_HandlerThrows_RedisplaysFormWithError()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)])
        {
            WriteException = new InvalidOperationException("Storage is offline."),
        };
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                    edit.Fields(fields => fields.Add(product => product.Name))
                );
            }
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/7");
        values["Entity.Name"] = "Fountain pen";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(document.RequiredElement(".validation-summary-errors").TextContent)
            .Contains("This record couldn't be saved. Try again.");
        await Assert
            .That(document.RequiredElement("input[name='Entity.Name']").GetAttribute("value"))
            .IsEqualTo("Fountain pen");
        await Assert.That(state.Products[0].Name).IsEqualTo("Notebook");
    }

    [Test]
    public async Task EditRow_HandlerThrows_RendersRowMessage()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)])
        {
            WriteException = new InvalidOperationException("Storage is offline."),
        };
        await using var sut = await StellarAdminTestHost.CreateAsync(
            state,
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                {
                    edit.Fields(fields => fields.Add(product => product.Name));
                    edit.UseInlineRow();
                });
                resource.AllowDelete();
            }
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products");
        values["rows[7].Name"] = "Fountain pen";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/editrow/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var rows = (await response.ReadRowsAsync()).QuerySelectorAll("tbody > tr");
        await Assert.That(rows[0].HasAttribute("data-editing")).IsTrue();
        await Assert
            .That(rows[1].RequiredElement("[role='alert']").TextContent.Trim())
            .IsEqualTo("This record couldn't be saved. Try again.");
    }
}
