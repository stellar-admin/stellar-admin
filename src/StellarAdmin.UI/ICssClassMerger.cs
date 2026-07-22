namespace StellarAdmin.UI;

public interface ICssClassMerger
{
    string? Merge(params string?[] classes);
}
