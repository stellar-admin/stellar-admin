using System.ComponentModel.DataAnnotations;

namespace DataGridSpike;

public record SpikeCrewMember(
    [property: Display(Name = "Crew member")] string FullName,
    string HomePort,
    int YearsAtSea
);
