using System.ComponentModel.DataAnnotations;

namespace VoyagerTravel.Models;

public enum TripDifficulty
{
    [Display(Name = "Easy", Description = "Gentle walks, a few hours a day.")]
    Easy,

    [Display(Name = "Moderate", Description = "Full days on foot, some climbs.")]
    Moderate,

    [Display(Name = "Challenging", Description = "Long days and steep trails.")]
    Challenging,
}
