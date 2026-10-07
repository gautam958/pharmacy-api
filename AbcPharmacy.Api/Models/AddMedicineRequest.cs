using System.ComponentModel.DataAnnotations;
using AbcPharmacy.Api.Validation;

namespace AbcPharmacy.Api.Models;

public class AddMedicineRequest
{
    [Required, StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Notes { get; set; }

    [Required]
    public DateOnly? ExpiryDate { get; set; }

    [Required, Range(0, 100000)]
    public int? Quantity { get; set; }

    [Required, Range(0.01, 1000000), MaxDecimalPlaces(2)]
    public decimal? Price { get; set; }

    [Required, StringLength(100)]
    public string Brand { get; set; } = string.Empty;
}
