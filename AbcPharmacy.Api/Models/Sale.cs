namespace AbcPharmacy.Api.Models;

public class Sale
{
    public int Id { get; set; }
    public int MedicineId { get; set; }

    // name and price are copied at the time of sale so the history doesn't change later
    public string MedicineName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime SoldOn { get; set; }
}
