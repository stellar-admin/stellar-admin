namespace AdminPlayground.Resources.Categories;

// The Categories index row details: the products in the category
public sealed record CategoryProducts(int Count, IReadOnlyList<CategoryProduct> Products);

public sealed record CategoryProduct(string Name, string Sku, decimal Price);
