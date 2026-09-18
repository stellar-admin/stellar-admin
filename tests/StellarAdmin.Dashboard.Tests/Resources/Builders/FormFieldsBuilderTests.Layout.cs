using StellarAdmin.Dashboard.Resources.Builders;
using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.Dashboard.Tests.Support;

namespace StellarAdmin.Dashboard.Tests.Resources.Builders;

public partial class FormFieldsBuilderTests
{
    [Test]
    public async Task AddSection_WhenContainersAreNested_PreservesSeededFieldOrderAndStructure()
    {
        // Arrange
        var options = new TestResourceOptions();
        var resource = new ResourceBuilder<TestEntity>(options);
        FormFieldsBuilder<TestEntity> sut = null!;
        resource.Create(page => page.Fields(fields => sut = fields));

        // Act
        sut.AddSection(
            "Details",
            section =>
            {
                section.Description = "Description";
                section.Fields(fields =>
                {
                    fields.Add(entity => entity.Sku);
                    fields.AddGroup(group =>
                        group.Fields(items =>
                            items.AddRow(row =>
                                row.Fields(columns =>
                                    columns.Add(entity => entity.CreatedAt).ReadOnly()
                                )
                            )
                        )
                    );
                });
            }
        );

        // Assert
        await Assert
            .That(options.CreatePage.Fields.Select(field => field.FieldName).ToArray())
            .IsEquivalentTo(new[] { "Name", "Sku", "CreatedAt" });
        await Assert
            .That(string.Join(",", options.CreatePage.Fields.Select(field => field.FieldName)))
            .IsEqualTo("Name,Sku,CreatedAt");
        await Assert.That(options.CreatePage.Items.Count).IsEqualTo(2);
        await Assert.That(options.CreatePage.Fields.Last().IsReadOnly).IsTrue();
    }

    [Test]
    public async Task Clear_WhenCalledOnNestedFields_PreservesParentAndSiblings()
    {
        // Arrange
        var options = new TestResourceOptions();
        var resource = new ResourceBuilder<TestEntity>(options);
        FormFieldsBuilder<TestEntity> sut = null!;
        resource.Create(page => page.Fields(fields => sut = fields));
        FormFieldsBuilder<TestEntity> nested = null!;
        sut.AddSection(
            "Details",
            section =>
                section.Fields(fields =>
                {
                    nested = fields;
                    fields.Add(entity => entity.Sku);
                })
        );

        // Act
        nested.Clear();
        nested.Add(entity => entity.Price);

        // Assert
        await Assert
            .That(string.Join(",", options.CreatePage.Fields.Select(field => field.FieldName)))
            .IsEqualTo("Name,Price");
        await Assert.That(options.CreatePage.Items.Count).IsEqualTo(2);
    }

    [Test]
    public async Task Clear_WhenRootHasContainers_RemovesAllItemsAndFields()
    {
        // Arrange
        var options = new TestResourceOptions();
        var resource = new ResourceBuilder<TestEntity>(options);
        FormFieldsBuilder<TestEntity> sut = null!;
        resource.Create(page => page.Fields(fields => sut = fields));
        sut.AddSection(
            "Details",
            section =>
            {
                section.Description = "Description";
                section.Fields(fields =>
                {
                    fields.Add(entity => entity.Sku);
                    fields.AddGroup(group =>
                        group.Fields(items =>
                            items.AddRow(row =>
                                row.Fields(columns =>
                                    columns.Add(entity => entity.CreatedAt).ReadOnly()
                                )
                            )
                        )
                    );
                });
            }
        );

        // Act
        sut.Clear();

        // Assert
        await Assert.That(options.CreatePage.Items).IsEmpty();
        await Assert.That(options.CreatePage.Fields).IsEmpty();
    }
}
