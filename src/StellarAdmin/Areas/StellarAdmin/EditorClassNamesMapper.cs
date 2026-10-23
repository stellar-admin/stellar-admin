using StellarAdmin.Diagnostics;
using StellarAdmin.Resources.Editors;
using StellarAdmin.UI;

namespace StellarAdmin.Areas.StellarAdmin;

internal static class EditorClassNamesMapper
{
    public static ChoiceGroupClassNames ForCheckboxGroup(FieldEditor? editor)
    {
        if (editor is not null && editor is not CheckboxGroupEditor)
        {
            throw new ConfigurationException(
                ConfigurationProblemKind.TemplateMismatch,
                $"its editor is a `{editor.TypeName}`, but it renders with the checkbox group template",
                "Render the field with its editor's own template, or give it a `CheckboxGroupEditor`."
            );
        }

        var classes = editor?.ClassNames;

        return new ChoiceGroupClassNames
        {
            Content = classes?.Content,
            Control = classes?.Control,
            Description = classes?.Description,
            Error = classes?.Error,
            Label = classes?.Label,
            Root = classes?.Root,
        };
    }

    public static IconPickerClassNames ForIconPicker(IconEditor editor, string? trigger = null)
    {
        var classes = editor.ClassNames;

        return new IconPickerClassNames
        {
            Content = classes.Content,
            Control = classes.Control,
            Description = classes.Description,
            Error = classes.Error,
            Label = classes.Label,
            Root = classes.Root,
            Trigger = trigger,
        };
    }

    public static ImageUploadClassNames ForImageUpload(ImageEditor editor)
    {
        var classes = editor.ClassNames;

        return new ImageUploadClassNames
        {
            Content = classes.Content,
            Control = classes.Control,
            Description = classes.Description,
            Error = classes.Error,
            Image = classes.Image,
            Label = classes.Label,
            Note = classes.Note,
            Root = classes.Root,
        };
    }

    public static InputClassNames ForInput(FieldEditor? editor)
    {
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

    public static InputOtpClassNames ForOneTimeCode(OneTimeCodeEditor editor)
    {
        var classes = editor.ClassNames;

        return new InputOtpClassNames
        {
            Content = classes.Content,
            Control = classes.Control,
            Description = classes.Description,
            Error = classes.Error,
            Label = classes.Label,
            Root = classes.Root,
        };
    }

    public static RadioGroupEditorClassNames ForRadio(FieldEditor? editor)
    {
        if (editor is not null && editor is not RadioGroupEditor)
        {
            throw new ConfigurationException(
                ConfigurationProblemKind.TemplateMismatch,
                $"its editor is a `{editor.TypeName}`, but it renders with the radio group template",
                "Render the field with its editor's own template, or give it a `RadioGroupEditor`."
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
            throw new ConfigurationException(
                ConfigurationProblemKind.TemplateMismatch,
                "its class names set `Content` or `Option.Error`, which a radio group doesn't have",
                "Set `Option.Content` or `Error` instead."
            );
        }

        return classes;
    }

    public static RichTextEditorClassNames ForRichTextEditor(RichTextEditor editor)
    {
        var classes = editor.ClassNames;

        return new RichTextEditorClassNames
        {
            Content = classes.Content,
            Control = classes.Control,
            Description = classes.Description,
            Error = classes.Error,
            Label = classes.Label,
            Root = classes.Root,
        };
    }

    public static SelectClassNames ForSelect(FieldEditor? editor)
    {
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

    public static SliderClassNames ForSlider(SliderEditor editor)
    {
        var classes = editor.ClassNames;

        return new SliderClassNames
        {
            Content = classes.Content,
            Control = classes.Control,
            Description = classes.Description,
            Error = classes.Error,
            Label = classes.Label,
            Mark = classes.Mark,
            MarkLabel = classes.MarkLabel,
            Marks = classes.Marks,
            Root = classes.Root,
        };
    }

    public static SwitchClassNames ForSwitch(ToggleEditor editor)
    {
        var classes = editor.ClassNames;

        return new SwitchClassNames
        {
            Content = classes.Content,
            Control = classes.Control,
            Description = classes.Description,
            Error = classes.Error,
            Label = classes.Label,
            Root = classes.Root,
        };
    }

    public static TextareaClassNames ForTextarea(FieldEditor? editor)
    {
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

    public static SegmentedControlClassNames ForSegmentedControl(SegmentedControlEditor editor)
    {
        var classes = editor.ClassNames;

        return new SegmentedControlClassNames
        {
            Content = classes.Content,
            Control = classes.Control,
            Description = classes.Description,
            Error = classes.Error,
            Label = classes.Label,
            Root = classes.Root,
        };
    }

    public static ToggleGroupClassNames ForToggleGroup(ToggleGroupEditor editor)
    {
        var classes = editor.ClassNames;

        return new ToggleGroupClassNames
        {
            Content = classes.Content,
            Control = classes.Control,
            Description = classes.Description,
            Error = classes.Error,
            Label = classes.Label,
            Root = classes.Root,
        };
    }
}
