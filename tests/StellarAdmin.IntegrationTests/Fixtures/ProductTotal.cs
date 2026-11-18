namespace StellarAdmin.IntegrationTests.Fixtures;

// The request a layout handler received, and the number of products
public sealed record ProductTotal(string? Layout, string? Scope, string? Search, int Products);
