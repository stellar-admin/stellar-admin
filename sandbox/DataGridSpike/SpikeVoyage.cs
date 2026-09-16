using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace DataGridSpike;

public record SpikeHarbor(
    string City,
    [property: DisplayFormat(DataFormatString = "{0:0.0} m")] decimal Depth,
    [property: UIHint("SpikeDate")] DateOnly? NextInspection
);

public record SpikeSkipper(
    [property: Display(Name = "Skipper")] string FullName,
    int YearsAtSea,
    SpikeHarbor? HomePort
);

public record SpikeVoyage(string Reference, SpikeSkipper? Skipper);
