using Microsoft.AspNetCore.Mvc;

namespace IdentitySimplePlayground.Data;

[ModelMetadataType<CategoryMeta>]
public class Category
{
    public string? Description { get; set; }

    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
