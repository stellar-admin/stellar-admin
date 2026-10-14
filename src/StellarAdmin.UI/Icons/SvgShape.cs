using System.Collections.Immutable;

namespace StellarAdmin.UI.Icons;

public record SvgShape(string Name, IImmutableDictionary<string, string> Attributes);
