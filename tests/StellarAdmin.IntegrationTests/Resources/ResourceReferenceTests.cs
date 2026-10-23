using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Editors;
using TUnit.Assertions.Enums;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourceReferenceTests
{
    [Test]
    public async Task Select_UseResource_ListsRecordsByTitleInDefaultSortOrder()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields
                .Add(model => model.SupplierId)
                .UseSelectEditor(select => select.UseResource<Supplier>())
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var options = document
            .RequiredElement("select[name='Entity.SupplierId']")
            .QuerySelectorAll("option")
            .Where(option => option.GetAttribute("value") != "")
            .ToArray();
        await Assert.That(options.Length).IsEqualTo(25);
        await Assert.That(options[0].GetAttribute("value")).IsEqualTo("1");
        await Assert.That(options[0].TextContent).IsEqualTo("Supplier 01");
        await Assert.That(options[^1].TextContent).IsEqualTo("Supplier 25");
    }

    [Test]
    public async Task Select_UseResourceWithoutTitle_IsLabelledByResourceLabel()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.SupplierId)
                    .UseSelectEditor(select => select.UseResource<Supplier>()),
            configureSuppliers: supplier => supplier.SingularLabel = "preferred supplier"
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.RequiredElement("label[for='Entity_SupplierId']").TextContent.Trim())
            .IsEqualTo("Preferred supplier");
    }

    [Test]
    public async Task Lookup_UseResource_ShowsSelectedTitleAndReferenceDescription()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.SupplierId)
                    .UseLookupSheetEditor(lookup => lookup.UseResource<Supplier>()),
            new() { SupplierId = 4 },
            supplier =>
                supplier.Reference(reference => reference.UseDescription(item => item.Country))
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var selected = document.RequiredElement("#Entity_SupplierId-selected");
        await Assert
            .That(selected.RequiredElement("[data-slot='item-title']").TextContent.Trim())
            .IsEqualTo("Supplier 04");
        await Assert
            .That(selected.RequiredElement("[data-slot='item-description']").TextContent)
            .IsEqualTo("Japan");
    }

    [Test]
    public async Task Lookup_Search_PassesTermPagingAndDefaultSortToDataSource()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields
                .Add(model => model.SupplierId)
                .UseLookupSheetEditor(lookup => lookup.UseResource<Supplier>())
        );
        using var client = sut.GetTestClient();
        var dataSource = sut.Services.GetRequiredService<SupplierDataSource>();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=create&field=SupplierId&term=supplier"
        );

        // Assert
        var request = dataSource.Requests.Last();
        await Assert.That(request.Search).IsEqualTo("supplier");
        await Assert.That(request.Paging).IsEqualTo(new ResourcePaging(1, 20));
        await Assert
            .That(request.Sort)
            .IsEqualTo(new ResourceSort(nameof(Supplier.Name), ResourceSortDirection.Ascending));
        var values = document
            .QuerySelectorAll("[data-lookup='item']")
            .Select(item => item.GetAttribute("data-value"))
            .ToArray();
        await Assert.That(values.Length).IsEqualTo(20);
        await Assert.That(values[0]).IsEqualTo("1");
        await Assert.That(document.QuerySelector("[data-lookup='more']")).IsNotNull();
    }

    [Test]
    public async Task Lookup_SearchLastPage_HasNoMoreLink()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields
                .Add(model => model.SupplierId)
                .UseLookupSheetEditor(lookup => lookup.UseResource<Supplier>())
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync(
            "/stellaradmin/products/lookup?form=create&field=SupplierId&skip=20"
        );

        // Assert
        await Assert.That(document.QuerySelectorAll("[data-lookup='item']").Length).IsEqualTo(5);
        await Assert.That(document.QuerySelector("[data-lookup='more']")).IsNull();
    }

    [Test]
    public async Task MultiLookup_UseResource_ShowsSelectedTitles()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.SupplierIds)
                    .UseMultiLookupSheetEditor(lookup => lookup.UseResource<Supplier>()),
            new() { SupplierIds = [3, 12] }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(
                document
                    .QuerySelectorAll("#Entity_SupplierIds-selected [data-lookup='item']")
                    .Select(chip => chip.TextContent.Trim())
            )
            .IsEquivalentTo(["Supplier 03", "Supplier 12"], CollectionOrdering.Matching);
    }

    [Test]
    public async Task UseTitle_NamesRecords()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.SupplierId)
                    .UseSelectEditor(select => select.UseResource<Supplier>()),
            configureSuppliers: supplier =>
                supplier.UseTitle(item => item.Name + " (" + item.Country + ")")
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.RequiredElement("option[value='2']").TextContent)
            .IsEqualTo("Supplier 02 (Japan)");
    }

    [Test]
    public async Task Title_DisplayColumnAndKey_NameRecordsWithoutUseTitle()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
            {
                fields
                    .Add(model => model.CarrierId)
                    .UseSelectEditor(select => select.UseResource<Carrier>());
                fields
                    .Add(model => model.DockCode)
                    .UseSelectEditor(select => select.UseResource<Dock>());
            },
            configureAdmin: admin =>
            {
                admin.AddResource<Carrier>(carrier =>
                {
                    carrier.UseDataSource<CarrierDataSource>();
                    carrier.UseKey(item => item.Id);
                });
                admin.AddResource<Dock>(dock =>
                {
                    dock.UseDataSource<DockDataSource>();
                    dock.UseKey(item => item.Code);
                });
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(
                document
                    .RequiredElement("select[name='Entity.CarrierId'] option[value='1']")
                    .TextContent
            )
            .IsEqualTo("Sea freight");
        await Assert
            .That(
                document
                    .RequiredElement("select[name='Entity.DockCode'] option[value='D-7']")
                    .TextContent
            )
            .IsEqualTo("D-7");
    }

    [Test]
    public async Task UseResource_NoResourceRegistered_LogsProblemAndShowsNoItems()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields
                .Add(model => model.CarrierId)
                .UseSelectEditor(select => select.UseResource<Carrier>())
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Message.Contains(
                        "A field that takes its items from Carrier has none because no resource is registered for Carrier."
                    )
                )
            )
            .IsTrue();
        await Assert
            .That(
                document
                    .RequiredElement("select[name='Entity.CarrierId']")
                    .QuerySelectorAll("option")
                    .Count(option => option.GetAttribute("value") != "")
            )
            .IsEqualTo(0);
    }

    [Test]
    public async Task UseResource_ResourceWithoutKey_LogsProblem()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.CarrierId)
                    .UseLookupSheetEditor(lookup => lookup.UseResource<Carrier>()),
            new() { CarrierId = 1 },
            configureAdmin: admin =>
                admin.AddResource<Carrier>(carrier => carrier.UseDataSource<CarrierDataSource>())
        );
        using var client = sut.GetTestClient();

        // Act
        using var response = await client.GetAsync("/stellaradmin/products/create");

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Message.Contains(
                        "A field that takes its items from Carrier has none because the Carrier resource has no key."
                    )
                )
            )
            .IsTrue();
    }

    [Test]
    public async Task UseResource_WhereOnDataSourceResource_LogsProblemAndListsEveryRecord()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields
                .Add(model => model.SupplierId)
                .UseSelectEditor(select =>
                    select.UseResource<Supplier>(items =>
                        items.Where((supplier, model) => supplier.Id < 3)
                    )
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var logs = sut.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Message.Contains(
                        "A field that takes its items from Supplier ignores its Where filter"
                    )
                )
            )
            .IsTrue();
        await Assert
            .That(
                document
                    .RequiredElement("select[name='Entity.SupplierId']")
                    .QuerySelectorAll("option")
                    .Count(option => option.GetAttribute("value") != "")
            )
            .IsEqualTo(25);
    }

    [Test]
    public async Task UseResource_ResourceRequiresAuthorization_ListsRecordsForOtherUsers()
    {
        // Arrange
        await using var sut = await CreateHost(
            fields =>
                fields
                    .Add(model => model.SupplierId)
                    .UseSelectEditor(select => select.UseResource<Supplier>()),
            configureSuppliers: supplier =>
                supplier.RequireAuthorization(policy => policy.RequireRole("Purchasing")),
            configureServices: TestAuthenticationHandler.Register
        );
        using var client = sut.GetTestClient();
        TestAuthenticationHandler.SignIn(client, "ada", []);

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        await Assert
            .That(document.RequiredElement("option[value='1']").TextContent)
            .IsEqualTo("Supplier 01");
    }

    [Test]
    public async Task CustomEditor_GetsItemsFromResourceReferences()
    {
        // Arrange
        await using var sut = await CreateHost(fields =>
            fields
                .Add(model => model.SupplierId)
                .UseEditor<SupplierChipsEditor>(chips => chips.UseResource<Supplier>())
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var chips = document.QuerySelectorAll("[data-supplier-chip]");
        await Assert.That(chips.Length).IsEqualTo(25);
        await Assert.That(chips[0].GetAttribute("data-supplier-chip")).IsEqualTo("1");
        await Assert.That(chips[0].TextContent).IsEqualTo("Supplier 01");
    }

    private static Task<WebApplication> CreateHost(
        Action<ResourceFieldsBuilder<ReferenceFieldsModel>> configureFields,
        ReferenceFieldsModel? model = null,
        Action<ResourceBuilder<Supplier>>? configureSuppliers = null,
        Action<StellarAdminBuilder>? configureAdmin = null,
        Action<IServiceCollection>? configureServices = null
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate<ReferenceFieldsModel, ReferenceFieldsHandler>(create =>
                {
                    create.UseFactory(() => model ?? new());
                    create.Fields(configureFields);
                }),
            admin =>
            {
                admin.AddResource<Supplier>(supplier =>
                {
                    supplier.UseDataSource<SupplierDataSource>();
                    supplier.UseKey(item => item.Id);
                    supplier.Index(index =>
                    {
                        index.Columns(columns =>
                            columns.Add(item => item.Name, column => column.Sortable())
                        );
                        index.DefaultSortBy(item => item.Name);
                    });
                    configureSuppliers?.Invoke(supplier);
                });
                configureAdmin?.Invoke(admin);
            },
            services =>
            {
                services.AddSingleton<SupplierStore>().AddSingleton<SupplierDataSource>();
                configureServices?.Invoke(services);
            }
        );
}
