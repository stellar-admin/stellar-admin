using Microsoft.EntityFrameworkCore.Metadata;

namespace StellarAdmin.Dashboard.EntityFrameworkCore;

internal static class EfCoreFormFieldMetadata
{
    public static IProperty? FindProperty(IEntityType entity, string path)
    {
        var segments = path.Split('.');
        ITypeBase current = entity;
        foreach (var segment in segments[..^1])
        {
            if (current.FindComplexProperty(segment) is { } complex)
            {
                current = complex.ComplexType;
                continue;
            }

            if (
                current is not IEntityType entityType
                || entityType.FindNavigation(segment)
                    is not { IsCollection: false, ForeignKey.IsOwnership: true } navigation
            )
            {
                return null;
            }

            current = navigation.TargetEntityType;
        }

        return current.FindProperty(segments[^1]);
    }
}
