using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using StellarAdmin.Pro.Areas.StellarAdmin;
using StellarAdmin.Pro.Resources.Builders;
using StellarAdmin.Pro.Resources.Options;

[Area("StellarAdmin")]
[Route("test-editors")]
public sealed class EditorTestController : Controller
{
    [HttpGet]
    public IActionResult Index(bool invalid = false, bool? choice = null, bool styled = false)
    {
        var fields = typeof(EditorTestModel)
            .GetProperties()
            .Select(property => property.Name)
            .ToArray();
        var options = new CreatePageOptions<EditorTestModel>(
            new FormPageDefaults("Editors", "Save", fields)
        );

        if (styled)
        {
            foreach (var field in options.Fields)
            {
                field.Editor.ClassNames.Root = $"test-editor-{field.FieldName}";
                field.Editor.ClassNames.Control = $"test-control-{field.FieldName}";
                field.Editor.ClassNames.Label = $"test-label-{field.FieldName}";
                field.Editor.ClassNames.Description = $"test-description-{field.FieldName}";
                field.Editor.ClassNames.Error = $"test-error-{field.FieldName}";
                field.Editor.ClassNames.Content = $"test-content-{field.FieldName}";
            }
        }

        ViewData[ViewDataKeys.FormFields] = options.Fields;
        if (invalid)
        {
            ModelState.SetModelValue(nameof(EditorTestModel.Decimal), "bad-number", "bad-number");
            ModelState.AddModelError(nameof(EditorTestModel.Decimal), "Enter a valid number.");
        }

        return View(
            "~/Areas/StellarAdmin/Views/Shared/_FormFields.cshtml",
            new EditorTestModel { NullableBoolean = choice }
        );
    }

    [HttpGet("radio")]
    public IActionResult Radio(
        string template = "EnumRadioGroup",
        bool typed = true,
        bool flags = false
    )
    {
        var options = new RadioResourceOptions();
        var resource = new ResourceBuilder<RadioTestModel>(options);
        resource.Create(create =>
            create.Fields(fields =>
            {
                var field = flags
                    ? fields.Add(model => model.Flags)
                    : fields.Add(model => model.Choice);
                field
                    .Template(template)
                    .Editor(editor =>
                    {
                        editor.ClassNames.Root = "radio-root";
                        editor.ClassNames.Control = "radio-options";
                        editor.ClassNames.Label = "radio-label";
                        editor.ClassNames.Description = "radio-description";
                        editor.ClassNames.Error = "radio-error";
                    });

                if (typed)
                {
                    field.Editor<RadioEditorOptions>(editor =>
                    {
                        editor.ClassNames.Option.Root = "option-root";
                        editor.ClassNames.Option.Control = "option-control";
                        editor.ClassNames.Option.Label = "option-label";
                        editor.ClassNames.Option.Content = "option-content";
                        editor.ClassNames.Option.Description = "option-description";
                    });
                }
            })
        );

        ModelState.AddModelError(flags ? "Flags" : "Choice", "Choose a condition.");
        ViewData[ViewDataKeys.FormFields] = options.CreatePage.Fields;

        return View("~/Areas/StellarAdmin/Views/Shared/_FormFields.cshtml", new RadioTestModel());
    }

    [HttpPost]
    public IActionResult Save(EditorTestModel model)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        return Json(model);
    }
}

public sealed class EditorTestModel
{
    public bool Boolean { get; set; } = true;

    public byte Byte { get; set; } = 12;

    [DataType(DataType.Currency)]
    public decimal Currency { get; set; } = 123.456m;

    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = new(2026, 9, 9);

    public DateOnly DateOnly { get; set; } = new(2026, 9, 9);

    public DateTime DateTime { get; set; } = new(2026, 9, 9, 14, 30, 15);

    public DateTimeOffset DateTimeOffset { get; set; } =
        new(2026, 9, 9, 14, 30, 15, TimeSpan.FromHours(7));

    [Display(Description = "A fractional amount")]
    public decimal Decimal { get; set; } = 123.456m;

    public double Double { get; set; } = 1.25;

    [DataType(DataType.EmailAddress)]
    public string Email { get; set; } = "person@example.com";

    public EditorTestStatus Enum { get; set; } = EditorTestStatus.Ready;

    public Guid Guid { get; set; } = Guid.Parse("ade5b738-dead-4567-abcd-912ba34c6789");

    public short Int16 { get; set; } = -12;

    public int Int32 { get; set; } = 42;

    public long Int64 { get; set; } = 9007199254740993;

    [DataType(DataType.MultilineText)]
    public string Multiline { get; set; } = "First line\nSecond line";

    public bool? NullableBoolean { get; set; }

    public DateOnly? NullableDateOnly { get; set; }

    public DateTime? NullableDateTime { get; set; }

    public decimal? NullableDecimal { get; set; }

    public EditorTestStatus? NullableEnum { get; set; }

    public TimeOnly? NullableTimeOnly { get; set; }

    [DataType(DataType.Password)]
    public string Password { get; set; } = "secret";

    [DataType(DataType.PhoneNumber)]
    public string Phone { get; set; } = "+123456789";

    [Editable(false)]
    public bool ReadOnlyBoolean { get; set; } = true;

    [Editable(false)]
    public EditorTestStatus ReadOnlyEnum { get; set; } = EditorTestStatus.Ready;

    public sbyte SByte { get; set; } = -12;

    public float Single { get; set; } = 1.25f;

    public string String { get; set; } = "Text";

    [DataType(DataType.Time)]
    public DateTime Time { get; set; } = new(2026, 9, 9, 14, 30, 15);

    public TimeOnly TimeOnly { get; set; } = new(14, 30, 15);

    public ushort UInt16 { get; set; } = 12;

    public uint UInt32 { get; set; } = 42;

    public ulong UInt64 { get; set; } = 18446744073709551615;

    [DataType(DataType.Url)]
    public string Url { get; set; } = "https://example.com";
}

public enum EditorTestStatus
{
    Draft,

    [Display(Name = "Ready to sell", Description = "Available for purchase.")]
    Ready,
}

public sealed class RadioTestModel
{
    [Display(Name = "Condition", Description = "Choose the item condition.")]
    public EditorTestStatus? Choice { get; set; } = EditorTestStatus.Ready;

    public EditorTestFlags Flags { get; set; } = EditorTestFlags.One;
}

[Flags]
public enum EditorTestFlags
{
    One = 1,
    Two = 2,
}

internal sealed class RadioResourceOptions()
    : ResourceOptions<RadioTestModel>(
        new IndexPageDefaults("Tests", "Create", "Empty", "", "database", [], null),
        new FormPageDefaults("Create", "Create", []),
        new FormPageDefaults("Edit", "Save", []),
        new DeleteDefaults<RadioTestModel>("Delete", "Delete?", "Delete", "Cancel", _ => null)
    );
