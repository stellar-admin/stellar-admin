using System.Net;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using StellarAdmin.IntegrationTests.Fixtures;
using StellarAdmin.IntegrationTests.Infrastructure;
using StellarAdmin.Resources.Builders;
using StellarAdmin.Resources.Displays;
using StellarAdmin.UI;
using static StellarAdmin.IntegrationTests.Infrastructure.FormTestHelpers;

namespace StellarAdmin.IntegrationTests.Resources;

public class ResourceDisplayFieldTests
{
    [Test]
    public async Task Create_WithDisplayField_ShowsEmptyText()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([]),
            resource =>
                resource.AllowCreate(create =>
                    create.Fields(fields =>
                    {
                        fields.Add(product => product.Price);
                        fields.AddDisplayField(
                            product => product.Name,
                            field => field.EmptyText = "Not named yet"
                        );
                    })
                )
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/create");

        // Assert
        var field = document.RequiredElement("[data-slot='display-field']");
        await Assert
            .That(field.RequiredElement("[data-slot='display-field-label']").TextContent.Trim())
            .IsEqualTo("Product name");
        await Assert
            .That(field.RequiredElement("[data-slot='display-field-value']").TextContent.Trim())
            .IsEqualTo("Not named yet");
        await Assert.That(document.QuerySelector("input[name='Entity.Name']")).IsNull();
    }

    [Test]
    public async Task CustomEdit_WithDisplayField_ReadsValueFromModel()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit<EditProductModel, EditProductHandler>(edit =>
                    edit.Fields(fields =>
                    {
                        fields.Add(model => model.ProductName);
                        fields
                            .AddDisplayField(model => model.Price)
                            .UseDisplay<NumberDisplay>(number => number.Format = "0.00");
                    })
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        await Assert
            .That(document.RequiredElement("[data-slot='display-field-value']").TextContent.Trim())
            .IsEqualTo("8.50");
    }

    [Test]
    public async Task Edit_WithBadgeDisplay_RendersBadgeWithVariant()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                        fields
                            .AddDisplayField(product => product.Name)
                            .UseDisplay<BadgeDisplay>(badge => badge.Variant = BadgeVariant.Outline)
                    )
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var badge = document.RequiredElement(
            "[data-slot='display-field-value'] [data-slot='badge']"
        );
        await Assert.That(badge.TextContent.Trim()).IsEqualTo("Notebook");
        await Assert.That(badge.ClassList.Contains("sa-badge-variant-outline")).IsTrue();
    }

    [Test]
    public async Task Edit_WithBadgeDisplayText_RendersTheText()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                        fields
                            .AddDisplayField(product => product.Name)
                            .UseDisplay<BadgeDisplay>(badge =>
                                badge.UseText<string>(name => name.ToUpperInvariant())
                            )
                    )
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var badge = document.RequiredElement(
            "[data-slot='display-field-value'] [data-slot='badge']"
        );
        await Assert.That(badge.TextContent.Trim()).IsEqualTo("NOTEBOOK");
    }

    [Test]
    public async Task Startup_WithBadgeDisplayTextOfAnotherType_LogsProblem()
    {
        // Arrange

        // Act
        await using var app = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                        fields
                            .AddDisplayField(product => product.Name)
                            .UseDisplay<BadgeDisplay>(badge =>
                                badge.UseText<int>(value => value.ToString())
                            )
                    )
                );
            }
        );

        // Assert
        var logs = app.Services.GetRequiredService<RecordingLoggerProvider>();
        await Assert
            .That(
                logs.ConfigurationProblems.Any(log =>
                    log.Message.Contains(
                        "because it shows String badges, and its UseText reads Int32."
                    )
                )
            )
            .IsTrue();
    }

    [Test]
    [Arguments(2, "Notebooks")]
    [Arguments(9, "None")]
    public async Task Edit_WithDisplayHandler_PassesPreparedDataToTemplate(int id, string expected)
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(id, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                        fields.AddDetails(
                            "Category",
                            details =>
                                details
                                    .Add(product => product.Id)
                                    .UseDisplay<CategoryNameDisplay>(display =>
                                        display.Missing = "None"
                                    )
                        )
                    )
                );
            },
            configureServices: services => services.AddSingleton<CategoryStore>()
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync($"/stellaradmin/products/edit/{id}");

        // Assert
        await Assert
            .That(document.RequiredElement("dd [data-custom-display='category']").TextContent)
            .IsEqualTo(expected);
    }

    [Test]
    public async Task Edit_WithoutDisplay_RendersDefaultDisplayForType()
    {
        // Arrange
        await using var sut = await CreateFactsHostAsync(details =>
        {
            details.Add(model => model.ProductName);
            details.Add(model => model.Notes);
            details.Add(model => model.Email);
            details.Add(model => model.Website);
            details.Add(model => model.Phone);
            details.Add(model => model.Released);
            details.Add(model => model.Updated);
            details.Add(model => model.Opens);
            details.Add(model => model.Active);
            details.Add(model => model.Discontinued);
            details.Add(model => model.Status);
            details.Add(model => model.Price);
            details.Add(model => model.Weight);
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var values = GetValues(document);
        var name = values["ProductName"].RequiredElement(".sa-text-display");
        await Assert.That(name.TextContent).IsEqualTo("Notebook");
        await Assert.That(name.GetAttribute("data-multiline")).IsEqualTo("false");
        await Assert.That(name.HasAttribute("data-line-clamp")).IsFalse();
        var notes = values["Notes"].RequiredElement(".sa-text-display");
        await Assert.That(notes.GetAttribute("data-multiline")).IsEqualTo("true");
        await Assert.That(notes.TextContent).IsEqualTo("Ships from Cape Town.\nAllow two days.");
        var email = values["Email"].RequiredElement("a");
        await Assert.That(email.GetAttribute("href")).IsEqualTo("mailto:sales@voyager.example");
        var website = values["Website"].RequiredElement("a");
        await Assert
            .That(website.GetAttribute("href"))
            .IsEqualTo("https://voyager.example/notebook");
        await Assert.That(website.GetAttribute("target")).IsEqualTo("_blank");
        await Assert.That(website.GetAttribute("rel")).IsEqualTo("noopener noreferrer");
        var phone = values["Phone"].RequiredElement("a");
        await Assert.That(phone.GetAttribute("href")).IsEqualTo("tel:+27215550100");
        await Assert.That(phone.TextContent).IsEqualTo("+27 21 555 0100");
        await AssertTime(values["Released"], "2026-03-14", "14 Mar 2026");
        await AssertTime(values["Updated"], "2026-10-12T14:05:00", "12 Oct 2026, 14:05");
        await AssertTime(values["Opens"], "09:30:00", "09:30");
        var active = values["Active"].RequiredElement(".sa-boolean-display");
        await Assert.That(active.GetAttribute("data-value")).IsEqualTo("true");
        await Assert.That(active.QuerySelector("svg")).IsNotNull();
        await Assert.That(active.RequiredElement(".sr-only").TextContent).IsEqualTo("Yes");
        await Assert
            .That(values["Discontinued"].RequiredElement(".sr-only").TextContent)
            .IsEqualTo("No");
        await Assert.That(values["Status"].TextContent.Trim()).IsEqualTo("On sale");
        await Assert
            .That(values["Price"].RequiredElement(".sa-number-display").TextContent)
            .IsEqualTo("8.50");
        await Assert
            .That(values["Weight"].RequiredElement(".sa-number-display").TextContent)
            .IsEqualTo("1.500");
    }

    [Test]
    public async Task Edit_WithDisplaySettings_RendersSettings()
    {
        // Arrange
        await using var sut = await CreateFactsHostAsync(details =>
        {
            details.Add(model => model.Notes).UseDisplay<TextDisplay>(text => text.LineClamp = 2);
            details
                .Add(model => model.Website)
                .UseDisplay<LinkDisplay>(link => link.OpenInNewTab = false);
            details
                .Add(model => model.Updated)
                .UseDisplay<DateDisplay>(date => date.Format = "yyyy/MM/dd");
            details
                .Add(model => model.Active)
                .UseDisplay<BooleanDisplay>(boolean =>
                {
                    boolean.Appearance = BooleanDisplayAppearance.Text;
                    boolean.TrueText = "Listed";
                });
            details
                .Add(model => model.Discontinued)
                .UseDisplay<BooleanDisplay>(boolean =>
                    boolean.Appearance = BooleanDisplayAppearance.IconAndText
                );
            details
                .Add(model => model.Weight)
                .UseDisplay<NumberDisplay>(number => number.Format = "0.0");
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var values = GetValues(document);
        var notes = values["Notes"].RequiredElement(".sa-text-display");
        await Assert.That(notes.GetAttribute("data-line-clamp")).IsEqualTo("2");
        await Assert.That(notes.GetAttribute("style")).IsEqualTo("--_line-clamp:2");
        await Assert.That(values["Website"].RequiredElement("a").HasAttribute("target")).IsFalse();
        await AssertTime(values["Updated"], "2026-10-12T14:05:00", "2026/10/12");
        var active = values["Active"].RequiredElement(".sa-boolean-display");
        await Assert.That(active.QuerySelector("svg")).IsNull();
        await Assert.That(active.TextContent.Trim()).IsEqualTo("Listed");
        var discontinued = values["Discontinued"].RequiredElement(".sa-boolean-display");
        await Assert.That(discontinued.QuerySelector("svg")).IsNotNull();
        await Assert.That(discontinued.QuerySelector(".sr-only")).IsNull();
        await Assert.That(discontinued.TextContent.Trim()).IsEqualTo("No");
        await Assert.That(values["Weight"].TextContent.Trim()).IsEqualTo("1.5");
    }

    [Test]
    public async Task Edit_WithImageDisplay_RendersFigureFromResolvedUrl()
    {
        // Arrange
        await using var sut = await CreateFactsHostAsync(details =>
        {
            details
                .Add(model => model.Photo)
                .UseDisplay<ImageDisplay>(image =>
                    image.ResolveUrl(value => $"https://cdn.voyager.example/{value}")
                );
            details
                .Add(model => model.Website)
                .UseDisplay<ImageDisplay>(image =>
                {
                    image.AspectRatio = ImageAspectRatio.Widescreen;
                    image.Size = ImageDisplaySize.Small;
                    image.Interaction = ImageDisplayInteraction.Preview;
                });
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var values = GetValues(document);
        var photo = values["Photo"].RequiredElement(".sa-image");
        await Assert.That(photo.LocalName).IsEqualTo("button");
        await Assert.That(photo.GetAttribute("data-variant")).IsEqualTo("figure");
        await Assert.That(photo.GetAttribute("data-size")).IsEqualTo("md");
        await Assert.That(photo.GetAttribute("data-aspect-ratio")).IsEqualTo("original");
        await Assert
            .That(photo.RequiredElement("img").GetAttribute("src"))
            .IsEqualTo("https://cdn.voyager.example/notebook.jpg");
        var website = values["Website"].RequiredElement(".sa-image");
        await Assert.That(website.LocalName).IsEqualTo("span");
        await Assert.That(website.GetAttribute("data-size")).IsEqualTo("sm");
        await Assert.That(website.GetAttribute("data-aspect-ratio")).IsEqualTo("widescreen");
        await Assert.That(website.GetAttribute("data-interaction")).IsEqualTo("preview");
        await Assert
            .That(website.RequiredElement("img").GetAttribute("src"))
            .IsEqualTo("https://voyager.example/notebook");
    }

    [Test]
    public async Task Edit_WithImageDisplayAndEmptyValue_RendersPlaceholder()
    {
        // Arrange
        await using var sut = await CreateFactsHostAsync(
            details =>
                details.Add(model => model.Photo).UseDisplay<ImageDisplay>().EmptyText = "None",
            model => model.Photo = null
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var value = GetValues(document)["Photo"];
        var image = value.RequiredElement(".sa-image");
        await Assert.That(value.HasAttribute("data-empty")).IsFalse();
        await Assert.That(image.GetAttribute("data-state")).IsEqualTo("empty");
        await Assert
            .That(image.RequiredElement("[data-slot='image-placeholder']").TextContent.Trim())
            .IsEqualTo("No image");
    }

    [Test]
    public async Task Edit_WithUIHint_RendersHintTemplateUnlessDisplayIsConfigured()
    {
        // Arrange
        await using var sut = await CreateFactsHostAsync(details =>
        {
            details.Add(model => model.Code);
            details.Add(
                model => model.Code,
                field =>
                {
                    field.Title = "Badge";
                    field.UseDisplay<BadgeDisplay>();
                }
            );
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var values = GetValues(document);
        await Assert
            .That(values["Code"].RequiredElement("[data-custom-display='code']").TextContent)
            .IsEqualTo("NB-7");
        await Assert
            .That(values["Badge"].RequiredElement("[data-slot='badge']").TextContent.Trim())
            .IsEqualTo("NB-7");
    }

    [Test]
    public async Task Edit_WithBuiltInRendering_RendersValueFromMetadata()
    {
        // Arrange
        await using var sut = await CreateFactsHostAsync(details =>
        {
            details.Add(model => model.Weight).UseBuiltInRendering();
            details.Add(model => model.Active).UseBuiltInRendering();
            details.Add(model => model.Reference);
        });
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var values = GetValues(document);
        await Assert.That(values["Weight"].GetAttribute("data-kind")).IsEqualTo("number");
        await Assert.That(values["Weight"].InnerHtml.Trim()).IsEqualTo("1.500");
        await Assert.That(values["Active"].GetAttribute("data-boolean")).IsEqualTo("true");
        await Assert
            .That(values["Reference"].InnerHtml.Trim())
            .IsEqualTo(ProductFactsHandler.Reference.ToString());
    }

    [Test]
    public async Task Edit_WithEmptyValue_ShowsEmptyTextInsteadOfDisplay()
    {
        // Arrange
        await using var sut = await CreateFactsHostAsync(
            details =>
            {
                details.Add(model => model.Email, field => field.EmptyText = "No email");
                details.Add(model => model.Discontinued);
            },
            model =>
            {
                model.Email = null;
                model.Discontinued = null;
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var values = GetValues(document);
        await Assert.That(values["Email"].GetAttribute("data-empty")).IsEqualTo("true");
        await Assert.That(values["Email"].TextContent.Trim()).IsEqualTo("No email");
        await Assert.That(values["Discontinued"].GetAttribute("data-empty")).IsEqualTo("true");
        await Assert.That(values["Discontinued"].QuerySelector(".sa-boolean-display")).IsNull();
    }

    [Test]
    public async Task Edit_WithComputedFields_RendersTheValuesOrEmptyText()
    {
        // Arrange
        await using var sut = await CreateFactsHostAsync(
            details =>
            {
                details.Add(
                    model => model.ProductName + " for " + model.Price,
                    field => field.Title = "Summary"
                );
                details.Add(
                    model => model.Email!.ToUpperInvariant(),
                    field =>
                    {
                        field.Title = "Contact";
                        field.EmptyText = "No email";
                    }
                );
            },
            model => model.Email = null
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var values = GetValues(document);
        await Assert.That(values["Summary"].TextContent.Trim()).StartsWith("Notebook for 8");
        await Assert.That(values["Contact"].TextContent.Trim()).IsEqualTo("No email");
    }

    [Test]
    public async Task Edit_WithDetails_RendersListWithLayoutAndPlacement()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                    {
                        fields.Add(product => product.Name);
                        fields.AddDetails(
                            "Audit",
                            details =>
                            {
                                details.Description = "Read-only values.";
                                details.Orientation = DetailsOrientation.Horizontal;
                                details.Variant = DetailsVariant.Divided;
                                details.Columns(3);
                                details.Add(
                                    product => product.Id,
                                    field => field.Title = "Reference"
                                );
                                details.Add(product => product.Price).ColumnSpan(2).ColumnStart(2);
                                details.Add(product => product.Name).ColumnSpanFull();
                            }
                        );
                    })
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var details = document.RequiredElement("[data-slot='details']");
        var section = details.Closest("[data-slot='form-section']")!;
        await Assert
            .That(section.RequiredElement("[data-slot='form-section-title']").TextContent.Trim())
            .IsEqualTo("Audit");
        await Assert.That(section.TextContent).Contains("Read-only values.");
        await Assert.That(details.GetAttribute("data-variant")).IsEqualTo("divided");
        await Assert
            .That(details.RequiredElement("dl").GetAttribute("style"))
            .IsEqualTo("--sa-cols-md:3;--sa-cols-lg:3");
        var fields = details.QuerySelectorAll("dl > [data-slot='display-field']");
        await Assert.That(fields.Length).IsEqualTo(3);
        await Assert.That(fields[0].GetAttribute("data-orientation")).IsEqualTo("horizontal");
        await Assert
            .That(fields[0].RequiredElement("dt").TextContent.Trim())
            .IsEqualTo("Reference");
        await Assert.That(fields[0].RequiredElement("dd").TextContent.Trim()).IsEqualTo("7");
        await Assert
            .That(fields[1].GetAttribute("style"))
            .IsEqualTo("--sa-span-md:2;--sa-span-lg:2;--sa-start-md:2;--sa-start-lg:2");
        await Assert
            .That(fields[2].GetAttribute("style"))
            .IsEqualTo("--sa-span-md:3;--sa-span-lg:3");
    }

    [Test]
    [Arguments(false, "EUR 8.50")]
    [Arguments(true, "USD 8.50")]
    public async Task Edit_WithDisplay_RendersDisplayTemplateWithSettings(
        bool configure,
        string expected
    )
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                    {
                        var price = fields.AddDisplayField(product => product.Price);
                        if (configure)
                        {
                            price.UseDisplay<PriceDisplay>(display => display.Currency = "USD");
                        }
                        else
                        {
                            price.UseDisplay<PriceDisplay>();
                        }

                        fields.AddDetails(
                            "Audit",
                            details =>
                                details
                                    .Add(product => product.Price)
                                    .UseDisplay<PriceDisplay>(display => display.Currency = "GBP")
                        );
                    })
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        await Assert
            .That(document.TextContents("[data-custom-display='price']"))
            .IsEquivalentTo([expected, "GBP 8.50"]);
        await Assert.That(document.QuerySelector("dd > [data-custom-display='price']")).IsNotNull();
    }

    [Test]
    public async Task Edit_WithDisplayFieldAmongInputs_RendersInGridItem()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit(edit =>
                    edit.Fields(fields =>
                    {
                        fields.Columns(2);
                        fields.Add(product => product.Name);
                        fields.AddDisplayField(product => product.Price).ColumnSpanFull();
                        fields.AddSection(
                            "Pricing",
                            section =>
                                section.AddDisplayField(
                                    product => product.Id,
                                    field => field.Title = "Reference"
                                )
                        );
                    })
                );
            }
        );
        using var client = sut.GetTestClient();

        // Act
        var document = await client.GetDocumentAsync("/stellaradmin/products/edit/7");

        // Assert
        var item = document.RequiredElement("[data-slot='display-field']").ParentElement!;
        await Assert.That(item.GetAttribute("data-slot")).IsEqualTo("grid-item");
        await Assert.That(item.GetAttribute("style")).IsEqualTo("--sa-span-md:2;--sa-span-lg:2");
        await Assert
            .That(item.RequiredElement("[data-slot='display-field-value']").TextContent.Trim())
            .IsEqualTo("8.50");
        await Assert.That(item.QuerySelector("dt")).IsNull();
        await Assert
            .That(
                document
                    .RequiredElement("[data-slot='form-section'] [data-slot='display-field-label']")
                    .TextContent.Trim()
            )
            .IsEqualTo("Reference");
        await Assert.That(document.QuerySelector("input[name='Entity.Price']")).IsNull();
    }

    [Test]
    public async Task InvalidSubmission_RedisplaysDisplayFieldValues()
    {
        // Arrange
        await using var sut = await StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            ConfigureEdit
        );
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/7");
        values["Entity.Name"] = "";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var document = await response.ReadDocumentAsync();
        await Assert
            .That(document.RequiredElement("[data-slot='display-field-value']").TextContent.Trim())
            .IsEqualTo("8.50");
    }

    [Test]
    public async Task Submission_IgnoresPostedDisplayFieldValue()
    {
        // Arrange
        var state = new ProductState([new(7, "Notebook", 8.50m)]);
        await using var sut = await StellarAdminTestHost.CreateAsync(state, ConfigureEdit);
        using var client = sut.GetTestClient();
        var values = await PrepareForm(client, "/stellaradmin/products/edit/7");
        values["Entity.Name"] = "Updated notebook";
        values["Entity.Price"] = "999";
        using var content = new FormUrlEncodedContent(values);

        // Act
        using var response = await client.PostAsync("/stellaradmin/products/edit/7", content);

        // Assert
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Redirect);
        await Assert.That(state.Products[0].Name).IsEqualTo("Updated notebook");
        await Assert.That(state.Products[0].Price).IsEqualTo(8.50m);
    }

    private static async Task AssertTime(IElement value, string machineValue, string text)
    {
        var time = value.RequiredElement("time");
        await Assert.That(time.GetAttribute("datetime")).IsEqualTo(machineValue);
        await Assert.That(time.TextContent).IsEqualTo(text);
    }

    private static Task<WebApplication> CreateFactsHostAsync(
        Action<ResourceDetailsBuilder<ProductFactsModel>> configure,
        Action<ProductFactsModel>? change = null
    ) =>
        StellarAdminTestHost.CreateAsync(
            new([new(7, "Notebook", 8.50m)]),
            resource =>
            {
                resource.UseKey(product => product.Id);
                resource.AllowEdit<ProductFactsModel, ProductFactsHandler>(edit =>
                    edit.Fields(fields =>
                    {
                        fields.Add(model => model.ProductName);
                        fields.AddDetails("Facts", configure);
                    })
                );
            },
            configureServices: services =>
                services.AddSingleton(new ProductFactsChange(change ?? (_ => { })))
        );

    // The value of each display field, by its label
    private static Dictionary<string, IElement> GetValues(IDocument document) =>
        document
            .QuerySelectorAll("[data-slot='display-field']")
            .ToDictionary(
                field =>
                    field.RequiredElement("[data-slot='display-field-label']").TextContent.Trim(),
                field => field.RequiredElement("[data-slot='display-field-value']")
            );

    private static void ConfigureEdit(ResourceBuilder<Product> resource)
    {
        resource.UseKey(product => product.Id);
        resource.AllowEdit(edit =>
            edit.Fields(fields =>
            {
                fields.Add(product => product.Name);
                fields
                    .AddDisplayField(product => product.Price)
                    .UseDisplay<NumberDisplay>(number => number.Format = "0.00");
            })
        );
    }
}
