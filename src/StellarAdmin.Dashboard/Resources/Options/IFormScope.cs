namespace StellarAdmin.Dashboard.Resources.Options;

internal interface IFormScope
{
    FormGridColumnDefinitions Columns { get; set; }

    IList<FormItemOptions> Items { get; }
}
