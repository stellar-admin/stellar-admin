using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin;

// Writes a choice group's column counts and, when the choices flow down, the row counts that fill each column before
// the next one starts.
internal static class ChoiceColumnsStyle
{
    public static string? Format(GridTiers columns, int choiceCount, bool down)
    {
        var columnsStyle = ResponsiveGrid.ColumnsStyle(columns);
        if (!down)
        {
            return columnsStyle;
        }

        var rowsStyle = ResponsiveGrid.RowsStyle(
            new GridTiers(
                Rows(choiceCount, columns.Default),
                Rows(choiceCount, columns.Small),
                Rows(choiceCount, columns.Medium),
                Rows(choiceCount, columns.Large)
            )
        );

        return columnsStyle is null ? rowsStyle
            : rowsStyle is null ? columnsStyle
            : $"{columnsStyle};{rowsStyle}";
    }

    private static int Rows(int choiceCount, int columns) =>
        Math.Max(1, (choiceCount + columns - 1) / columns);
}
