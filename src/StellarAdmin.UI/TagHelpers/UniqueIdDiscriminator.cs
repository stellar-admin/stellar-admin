namespace StellarAdmin.UI;

// Keeps generated element ids unique when a container, such as a data grid that renders its column
// templates for each row, renders the same markup more than once. The container shares an instance
// with SetContext and changes Value before each pass; GetUniqueId adds it to each id.
internal sealed class UniqueIdDiscriminator
{
    public string? Value { get; set; }
}
