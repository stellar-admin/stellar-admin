using System.ComponentModel.DataAnnotations;

namespace DashboardPlayground.Resources.FieldEditors;

// Each property is one date or time scenario, and its display name describes the scenario.
public sealed class DateTimeGallery
    : FieldEditorGalleryRecord,
        IFieldEditorGalleryRecord<DateTimeGallery>
{
    // Data-type templates forwarding to the date and time inputs

    [Display(Name = "DateOnly")]
    public DateOnly Departure { get; set; }

    [Display(Name = "DateOnly?")]
    public DateOnly? OptionalDeparture { get; set; }

    [Required]
    [Display(Name = "DateOnly? · [Required]")]
    public DateOnly? RequiredDeparture { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "DateTime · [DataType(Date)]")]
    public DateTime DepartureDate { get; set; }

    [Display(Name = "DateTime")]
    public DateTime Boarding { get; set; }

    [Display(Name = "DateTime?")]
    public DateTime? OptionalBoarding { get; set; }

    [Display(Name = "DateTimeOffset")]
    public DateTimeOffset Booked { get; set; }

    [Display(Name = "TimeOnly")]
    public TimeOnly Gate { get; set; }

    [DataType(DataType.Time)]
    [Display(Name = "DateTime · [DataType(Time)]")]
    public DateTime GateTime { get; set; }

    // Field configuration

    [Display(Name = "DateOnly · this display name is replaced by the field Title")]
    public DateOnly TitledDeparture { get; set; }

    [Display(Name = "DateOnly · [Display(Description)]", Description = "Shown below the input.")]
    public DateOnly DescribedDeparture { get; set; }

    [Display(Name = "DateOnly · field Description")]
    public DateOnly FieldDescribedDeparture { get; set; }

    [Editable(false)]
    [Display(Name = "DateOnly · [Editable(false)]")]
    public DateOnly ReadOnlyDeparture { get; set; }

    [Editable(false)]
    [Display(Name = "DateTime · [Editable(false)]")]
    public DateTime ReadOnlyBoarding { get; set; }

    // Editor settings

    [Display(Name = "DateOnly · Min 2026-01-01 · Max 2026-12-31")]
    public DateOnly BoundedDeparture { get; set; }

    [Display(Name = "DateOnly · Step 7 days")]
    public DateOnly WeeklyDeparture { get; set; }

    [Display(Name = "DateTime · Step 15 minutes")]
    public DateTime QuarterHourBoarding { get; set; }

    [Display(Name = "TimeOnly · Min 08:00 · Max 18:00 · Step 30 minutes")]
    public TimeOnly OfficeHoursGate { get; set; }

    [Display(Name = "DateOnly · ClassNames.Control")]
    public DateOnly StyledDeparture { get; set; }

    public static DateTimeGallery CreateSample() =>
        new()
        {
            Departure = new DateOnly(2026, 10, 14),
            OptionalDeparture = null,
            RequiredDeparture = new DateOnly(2026, 11, 2),
            DepartureDate = new DateTime(2026, 10, 14),
            Boarding = new DateTime(2026, 10, 14, 9, 35, 0),
            OptionalBoarding = null,
            Booked = new DateTimeOffset(2026, 9, 1, 16, 20, 0, TimeSpan.FromHours(2)),
            Gate = new TimeOnly(9, 5),
            GateTime = new DateTime(2026, 10, 14, 9, 5, 0),
            TitledDeparture = new DateOnly(2026, 10, 15),
            DescribedDeparture = new DateOnly(2026, 10, 16),
            FieldDescribedDeparture = new DateOnly(2026, 10, 17),
            ReadOnlyDeparture = new DateOnly(2026, 10, 18),
            ReadOnlyBoarding = new DateTime(2026, 10, 18, 7, 45, 0),
            BoundedDeparture = new DateOnly(2026, 6, 1),
            WeeklyDeparture = new DateOnly(2026, 10, 15),
            QuarterHourBoarding = new DateTime(2026, 10, 14, 9, 30, 0),
            OfficeHoursGate = new TimeOnly(10, 30),
            StyledDeparture = new DateOnly(2026, 10, 20),
        };
}
