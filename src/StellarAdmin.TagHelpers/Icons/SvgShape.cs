using System.Collections.Immutable;

namespace StellarAdmin.TagHelpers.Icons;

public record SvgShape(string Name, IImmutableDictionary<string, string> Attributes);
