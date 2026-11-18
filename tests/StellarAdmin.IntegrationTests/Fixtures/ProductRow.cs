namespace StellarAdmin.IntegrationTests.Fixtures;

// A row of a table whose handler loads it: a product, its category, or neither when the ids are null
public sealed record ProductRow(int? ProductId, int? CategoryId, string Label, decimal Price);
