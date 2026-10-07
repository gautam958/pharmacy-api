namespace AbcPharmacy.Api.Services;

public class InventorySettings
{
    public int ExpiryWarningDays { get; set; } = 30;
    public int LowStockThreshold { get; set; } = 10;
}
