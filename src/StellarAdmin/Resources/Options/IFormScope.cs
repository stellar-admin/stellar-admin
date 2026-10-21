using StellarAdmin.Diagnostics;

namespace StellarAdmin.Resources.Options;

internal interface IFormScope
{
    FormGridColumnDefinitions Columns { get; set; }

    IList<FormItemOptions> Items { get; }

    ConfigurationSource? Source { get; }
}
