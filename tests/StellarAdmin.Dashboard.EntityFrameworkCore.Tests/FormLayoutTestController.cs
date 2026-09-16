using IdentitySimplePlayground.Data;
using Microsoft.AspNetCore.Mvc;
using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Controllers;
using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

[Area("StellarAdmin")]
[Route("test-form-layout")]
public sealed class FormLayoutTestController : ResourceControllerBase<Product>
{
    [HttpGet]
    public IActionResult Index(bool edit = false, FormSectionLayout? form = null)
    {
        var options = new LayoutResourceOptions();
        var resource = new ResourceBuilder<Product>(options);
        resource.Create(create =>
        {
            create.SectionLayout = form;
            create.Fields(ConfigureFields);
        });
        resource.Edit(page =>
        {
            page.SectionLayout = form;
            page.Fields(ConfigureFields);
        });

        return View(
            "~/Areas/StellarAdmin/Views/Shared/_FormPage.cshtml",
            BuildFormPageViewModel(edit ? options.EditPage : options.CreatePage, new Product())
        );

        void ConfigureFields(FormFieldsBuilder<Product> fields)
        {
            fields.AddGroup(group =>
                group.Fields(children =>
                    children.AddSection(
                        "Details",
                        builder =>
                        {
                            builder.Fields(items =>
                                items.AddRow(row =>
                                    row.Fields(columns =>
                                    {
                                        columns.Add(product => product.Name);
                                        columns.Add(product => product.Sku);
                                    })
                                )
                            );
                        }
                    )
                )
            );
        }
    }

    private sealed class LayoutResourceOptions()
        : ResourceOptions<Product>(
            new IndexPageDefaults("Products", "Create", "Empty", "", "database", [], null),
            new FormPageDefaults("Create", "Create", []),
            new FormPageDefaults("Edit", "Save", []),
            new DeleteDefaults<Product>("Delete", "Delete?", "Delete", "Cancel", _ => null)
        );
}
