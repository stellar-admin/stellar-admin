using StellarAdmin.Dashboard.Resources.Editors;
using StellarAdmin.Dashboard.Resources.Options;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin;

internal static class EditorClassNamesMapper
{
    public static InputClassNames ForInput(EditorOptions? options)
    {
        ValidateScalar(options);
        var classes = options?.ClassNames;

        return new InputClassNames
        {
            Content = classes?.Content,
            Control = classes?.Control,
            Description = classes?.Description,
            Error = classes?.Error,
            Label = classes?.Label,
            Root = classes?.Root,
        };
    }

    public static RadioEditorClassNames ForRadio(EditorOptions? options)
    {
        if (
            options is not null
            && options.GetType() != typeof(EditorOptions)
            && options is not RadioEditorOptions
        )
        {
            throw new InvalidOperationException(
                $"{options.GetType().Name} is not supported by a radio editor."
            );
        }

        var classes = options is RadioEditorOptions radio
            ? radio.ClassNames
            : new RadioEditorClassNames();
        if (options is not null && options is not RadioEditorOptions)
        {
            options.ClassNames.CopyTo(classes);
        }

        if (classes.Content is not null || classes.Option.Error is not null)
        {
            throw new InvalidOperationException(
                "Radio editors have no outer Content or per-option Error element. Use Option.Content or Error instead."
            );
        }

        return classes;
    }

    public static SelectClassNames ForSelect(EditorOptions? options)
    {
        ValidateScalar(options);
        var classes = options?.ClassNames;

        return new SelectClassNames
        {
            Content = classes?.Content,
            Control = classes?.Control,
            Description = classes?.Description,
            Error = classes?.Error,
            Label = classes?.Label,
            Root = classes?.Root,
        };
    }

    public static TextareaClassNames ForTextarea(EditorOptions? options)
    {
        ValidateScalar(options);
        var classes = options?.ClassNames;

        return new TextareaClassNames
        {
            Content = classes?.Content,
            Control = classes?.Control,
            Description = classes?.Description,
            Error = classes?.Error,
            Label = classes?.Label,
            Root = classes?.Root,
        };
    }

    private static void ValidateScalar(EditorOptions? options)
    {
        if (
            options is not null
            && options.GetType() != typeof(EditorOptions)
            && options is not ResourceEditor
        )
        {
            throw new InvalidOperationException(
                $"{options.GetType().Name} requires a compatible editor template. Scalar editors, including flags-enum text fallbacks, accept EditorOptions."
            );
        }
    }
}
