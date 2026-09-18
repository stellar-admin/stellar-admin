using StellarAdmin.Dashboard.Resources.Options;

namespace StellarAdmin.Dashboard.Tests.Support;

internal sealed class TestResourceOptions()
    : ResourceOptions<TestEntity>(
        new IndexPageDefaults("Tests", "Create", "Empty", "", "database", [], null),
        new FormPageDefaults("Create", "Create", ["Name"]),
        new FormPageDefaults("Edit", "Save", []),
        new DeleteDefaults<TestEntity>("Delete", "Delete?", "Delete", "Cancel", _ => null)
    );
