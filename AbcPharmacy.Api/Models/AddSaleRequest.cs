using System.ComponentModel.DataAnnotations;

namespace AbcPharmacy.Api.Models;

public class AddSaleRequest
{
    [Required]
    public int? MedicineId { get; set; }

    [Required, Range(1, 10000)]
    public int? Quantity { get; set; }
}
