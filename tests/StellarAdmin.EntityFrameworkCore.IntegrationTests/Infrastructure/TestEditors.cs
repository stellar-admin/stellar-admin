using StellarAdmin.Resources.Editors;

namespace StellarAdmin.EntityFrameworkCore.IntegrationTests.Infrastructure;

// The built-in editors that take items are created by the field builder's methods, so tests create them the same way
internal static class TestEditors
{
    public static TEditor Create<TEditor>()
        where TEditor : FieldEditor => (TEditor)Activator.CreateInstance(typeof(TEditor), nonPublic: true)!;
}
