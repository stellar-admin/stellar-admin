using StellarAdmin.Dashboard.Resources.Editors;
using StellarAdmin.TagHelpers;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin;

internal static class EditorClassNamesMapper
{
    public static string? ForCheckboxGroup(FieldEditor? editor)
    {
        if (
            editor is not null
            && editor.GetType() != typeof(FieldEditor)
            && editor is not CheckboxGroupEditor
        )
        {
            throw new InvalidOperationException(
                $"{editor.GetType().Name} is not supported by a checkbox group editor."
            );
        }

        var classes = editor?.ClassNames;
        if (
            classes?.Content is not null
            || classes?.Description is not null
            || classes?.Error is not null
            || classes?.Label is not null
            || classes?.Root is not null
        )
        {
            throw new InvalidOperationException(
                "Checkbox group editors support only Control, which styles the choices container."
            );
        }

        return classes?.Control;
    }

    public static InputClassNames ForInput(FieldEditor? editor)
    {
        ValidateScalar(editor);
        var classes = editor?.ClassNames;

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

    public static RadioGroupEditorClassNames ForRadio(FieldEditor? editor)
    {
        if (
            editor is not null
            && editor.GetType() != typeof(FieldEditor)
            && editor is not RadioGroupEditor
        )
        {
            throw new InvalidOperationException(
                $"{editor.GetType().Name} is not supported by a radio editor."
            );
        }

        var classes = editor is RadioGroupEditor radio
            ? radio.ClassNames
            : new RadioGroupEditorClassNames();
        if (editor is not null && editor is not RadioGroupEditor)
        {
            editor.ClassNames.CopyTo(classes);
        }

        if (classes.Content is not null || classes.Option.Error is not null)
        {
            throw new InvalidOperationException(
                "Radio editors have no outer Content or per-option Error element. Use Option.Content or Error instead."
            );
        }

        return classes;
    }

    public static SelectClassNames ForSelect(FieldEditor? editor)
    {
        ValidateScalar(editor);
        var classes = editor?.ClassNames;

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

    public static TextareaClassNames ForTextarea(FieldEditor? editor)
    {
        ValidateScalar(editor);
        var classes = editor?.ClassNames;

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

    private static void ValidateScalar(FieldEditor? editor)
    {
        if (
            editor is not null
            && editor.GetType() != typeof(FieldEditor)
            && editor is not IFieldEditor
        )
        {
            throw new InvalidOperationException(
                $"{editor.GetType().Name} requires a compatible editor template. Scalar editors, including flags-enum text fallbacks, accept FieldEditor."
            );
        }
    }
}
