namespace StellarAdmin.Resources.Options;

// A table row links to the view page of another resource's record, whose key the row holds. A null key is a row
// without a link
internal sealed record ResourceRowLink(Type ResourceType, Func<object, string?> Key);
