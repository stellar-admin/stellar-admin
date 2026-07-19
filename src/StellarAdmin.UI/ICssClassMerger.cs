using StellarAdmin.UI.Theming;

namespace StellarAdmin.UI;

public interface ICssClassMerger
{
    string? Merge(params ClassElement?[] classes);
}
