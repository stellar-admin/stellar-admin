using DocsSamples.Pages.Field;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DocsSamples.Pages;

public class DocsStatic : PageModel
{
    public string? Layout { get; set; }

    public object? PartialModel { get; set; }

    public string PartialName { get; set; }

    public void OnGet(string name, string? layout)
    {
        Layout = layout;
        PartialName = name;

        PartialModel = name switch
        {
            "Checkbox/_GroupModelBinding" => new Checkbox.Index.GroupModel(),
            "Checkbox/_ModelBinding" => new Checkbox.Index.ModelBindingModel(),
            "CheckboxGroup/_CollectionBinding" => new CheckboxGroup.Index.OrderModel(),
            "CheckboxGroup/_Validation" => new CheckboxGroup.Index.ValidationModel(),
            "Checkbox/_Validation" => new Checkbox.Index.ValidationModel(),
            "DictionaryEditor/_Intro" => new DictionaryEditor.Index.LocalNamesModel(),
            "DictionaryEditor/_EnumKeys"
            or "DictionaryEditor/_ReadOnly"
            or "DictionaryEditor/_Disabled" => new DictionaryEditor.Index.SeasonalPricesModel(),
            "DictionaryEditor/_Items" => new DictionaryEditor.Index.GuidesModel(),
            "DictionaryEditor/_Numbers" => new DictionaryEditor.Index.BaggageModel(),
            "DictionaryEditor/_Validation" => new DictionaryEditor.Index.IndexModel().Validation,
            "Details/_Columns"
            or "Details/_Divided"
            or "Details/_EmptyValues"
            or "Details/_Horizontal"
            or "Details/_InForm"
            or "Details/_Intro"
            or "Details/_RecordPage"
            or "Details/_Starts"
            or "Details/_Templates"
            or "Details/_Values" => Details.Index.SampleBooking,
            "Grid/_DisplayFields"
            or "Grid/_GridItems"
            or "Grid/_Intro"
            or "Grid/_Narrow"
            or "Grid/_Spans"
            or "Grid/_Starts" => Grid.Index.SampleBooking,
            "Field/_CheckboxImplicit" => new CheckboxImplicitModel { SyncMapWithItinerary = true },
            "Field/_FieldGroupImplicit" => new FieldGroupImplicitModel
            {
                EnableBookingPushNotifications = true,
            },
            "Field/_FieldsetImplicit" => new FieldsetImplicitModel(),
            "Field/_Implicit" => new ImplicitModel(),
            "Field/_ClassNames" or "Field/_InputImplicit" => new InputImplicitModel(),
            "Field/_RadioImplicit" => new RadioImplicitModel { InsuranceCover = "annual" },
            "Field/_SelectImplicit" => new SelectImplicitModel { TravelStyle = "beach" },
            "Field/_TextareaImplicit" => new TextareaImplicitModel(),
            "IconPicker/_ModelBinding" => new IconPicker.Index.ModelBindingModel(),
            "Input/_ModelBinding" => new Pages.Input.Index.ModelBindingModel(),
            "Input/_InputTypesModelBinding" => new Pages.Input.Index.InputTypesModelBindingModel(),
            "Input/_Validation" => new Input.Index.ValidationModel(),
            "InputOtp/_ModelBinding" => new Pages.InputOtp.Index.ModelBindingModel(),
            "InputOtp/_Validation" => new Pages.InputOtp.Index.ValidationModel(),
            "Label/_ModelBinding" => new Label.Index.ModelBindingModel(),
            "ListEditor/_Affixes" => new ListEditor.Index.TrailModel(),
            "ListEditor/_Empty" => new ListEditor.Index.PackingModel(),
            "ListEditor/_Intro"
            or "ListEditor/_Numbered"
            or "ListEditor/_ReadOnly"
            or "ListEditor/_Disabled"
            or "ListEditor/_Validation" => new ListEditor.Index.TripModel(),
            "ListEditor/_ItemChecks" => new ListEditor.Index.AirportsModel(),
            "ListEditor/_Numbers" => new ListEditor.Index.DeparturesModel(),
            "Progress/_FileUploadList" => Pages.Progress.Index.Files,
            "Questionnaire/_Card" => new Questionnaire.Index.CardModel(),
            "Questionnaire/_CustomProgress" => new Questionnaire.Index.CustomProgressModel(),
            "Questionnaire/_Freeform" => new Questionnaire.Index.FreeformModel(),
            "Questionnaire/_Intro" => new Questionnaire.Index.IntroModel(),
            "Questionnaire/_LongForm" => new Questionnaire.Index.LongFormModel(),
            "Questionnaire/_Multiple" => new Questionnaire.Index.MultipleModel(),
            "Questionnaire/_Shortcuts" => new Questionnaire.Index.ShortcutsModel(),
            "Questionnaire/_Steps" => new Questionnaire.Index.StepsModel(),
            "Questionnaire/_Validation" => new Questionnaire.Index.ValidationModel(),
            "Radio/_ModelBinding" => new Radio.Index.ModelBindingModel(),
            "RadioGroup/_ModelBinding" => new RadioGroup.Index.OrderModel(),
            "RichTextEditor/_ModelBinding" => new RichTextEditor.Index.GuideModel(),
            "RadioGroup/_Validation" => new RadioGroup.Index.ValidationModel(),
            "Radio/_Validation" => new Radio.Index.ValidationModel(),
            "SegmentedControl/_Validation" => new SegmentedControl.Index.BookingModel
            {
                Cabin = null,
            },
            "SegmentedControl/_ModelBinding" => new SegmentedControl.Index.BookingModel(),
            "Select/_ModelBinding" => new Select.Index.BookingFormModel(),
            "Select/_Validation" => new Select.Index.BookingFormValidationModel(),
            "Slider/_ModelBinding" => new Slider.Index.ModelBindingModel(),
            "Slider/_Value" => new Slider.Index.ValueModel(),
            "Switch/_ModelBinding" => new Switch.Index.ModelBindingModel(),
            "Textarea/_ModelBinding" => new Textarea.Index.ReviewModel(),
            "Textarea/_Validation" => new Textarea.Index.ReviewValidationModel(),
            "Toggle/_ModelBinding" => new Toggle.Index.ModelBindingModel(),
            "ToggleGroup/_ModelBinding" => new ToggleGroup.Index.ModelBindingModel(),
            _ => null,
        };

        // DocsStatic renders the partial with the "PartialModel" prefix (see DocsStatic.cshtml),
        // so validation errors must be keyed to the fully-qualified field name to attach to the input.
        switch (name)
        {
            case "Checkbox/_Validation":
                ModelState.AddModelError(
                    "PartialModel.AcceptTerms",
                    "You must accept the terms and conditions"
                );
                break;
            case "CheckboxGroup/_Validation":
                ModelState.AddModelError("PartialModel.Extras", "Choose at least one extra.");
                break;
            case "RadioGroup/_Validation":
                ModelState.AddModelError(
                    "PartialModel.DeliveryMethod",
                    "Choose a delivery method."
                );
                break;
            case "Input/_Validation":
                ModelState.AddModelError("PartialModel.Email", "Enter your email address");
                break;
            case "InputOtp/_Validation":
                ModelState.AddModelError(
                    "PartialModel.OneTimePassword",
                    "Enter the complete 6-digit code"
                );
                break;
            case "Questionnaire/_Validation":
                ModelState.AddModelError(
                    "PartialModel.CabinClass",
                    "Choose a cabin class to continue."
                );
                break;
            case "Radio/_Validation":
                ModelState.AddModelError("PartialModel.BedType", "Please select a bed type");
                break;
            case "SegmentedControl/_Validation":
                ModelState.AddModelError("PartialModel.Cabin", "Choose a cabin class.");
                break;
            case "Select/_Validation":
                ModelState.AddModelError(
                    "PartialModel.CabinClass",
                    "Please select a valid cabin class"
                );
                break;
            case "DictionaryEditor/_Validation":
                DictionaryEditor.Index.AddValidationErrors(ModelState, "PartialModel");
                break;
            case "ListEditor/_Validation":
                ListEditor.Index.AddValidationErrors(ModelState, "PartialModel");
                break;
            case "DataGrid/_EditingErrors":
                DataGrid.Index.AddEditingErrors(ModelState);
                break;
            case "Textarea/_Validation":
                ModelState.AddModelError(
                    "PartialModel.Review",
                    "Please leave a review for other travelers"
                );
                break;
        }
    }
}
