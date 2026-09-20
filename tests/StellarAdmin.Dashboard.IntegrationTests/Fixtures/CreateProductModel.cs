using System.ComponentModel.DataAnnotations;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class CreateProductModel(decimal price)
{
    public int Id { get; set; }

    [Required]
    public string ProductName { get; set; } = "";

    [Range(typeof(decimal), "0.01", "1000000")]
    public decimal Price { get; set; } = price;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string PasswordConfirmation { get; set; } = "";
}
