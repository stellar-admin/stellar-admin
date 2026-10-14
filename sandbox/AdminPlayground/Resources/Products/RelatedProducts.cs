namespace AdminPlayground.Resources.Products;

// The Products index Related panel: other products in the product's category
public sealed record RelatedProducts(string? Category, IReadOnlyList<RelatedProduct> Products);

public sealed record RelatedProduct(string Name, string Sku, decimal Price);
