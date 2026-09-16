using System.ComponentModel.DataAnnotations;

namespace IdentitySimplePlayground.Data;

public enum ProductCondition
{
    [Display(Name = "New", Description = "An unused item in its original condition.")]
    New = 0,

    [Display(
        Name = "Refurbished",
        Description = "A previously owned item restored and tested for resale."
    )]
    Refurbished = 1,

    [Display(Name = "Used", Description = "A previously owned item that may show signs of wear.")]
    Used = 2,
}
