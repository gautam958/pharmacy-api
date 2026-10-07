namespace AbcPharmacy.Api.Models;

// What the grid gets back. Notes are not part of the list.
public class MedicineDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public DateOnly ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public bool IsExpiringSoon { get; set; }
    public bool IsLowStock { get; set; }
    public bool IsExpired { get; set; }
}
