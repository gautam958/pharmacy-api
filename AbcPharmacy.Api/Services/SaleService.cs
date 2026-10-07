using AbcPharmacy.Api.Data;
using AbcPharmacy.Api.Models;

namespace AbcPharmacy.Api.Services;

public interface ISaleService
{
    PagedResult<Sale> GetSales(int page, int pageSize);
    Sale AddSale(AddSaleRequest request);
}

public class SaleService : ISaleService
{
    private readonly JsonDataStore _store;

    public SaleService(JsonDataStore store)
    {
        _store = store;
    }

    public PagedResult<Sale> GetSales(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 500);

        return _store.Read((_, sales) => new PagedResult<Sale>
        {
            TotalCount = sales.Count,
            Page = page,
            PageSize = pageSize,
            Items = sales.OrderByDescending(s => s.SoldOn).Skip((page - 1) * pageSize).Take(pageSize).ToList()
        });
    }

    public Sale AddSale(AddSaleRequest request)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var quantity = request.Quantity!.Value;

        // stock check and stock update happen inside the same lock
        return _store.Write((medicines, sales) =>
        {
            var medicine = medicines.FirstOrDefault(m => m.Id == request.MedicineId)
                ?? throw new NotFoundException($"Medicine {request.MedicineId} not found.");

            if (medicine.ExpiryDate < today)
            {
                throw new BusinessException($"{medicine.FullName} is expired and cannot be sold.");
            }

            if (medicine.Quantity < quantity)
            {
                throw new BusinessException($"Only {medicine.Quantity} left in stock for {medicine.FullName}.");
            }

            medicine.Quantity -= quantity;

            var sale = new Sale
            {
                Id = sales.Count == 0 ? 1 : sales.Max(s => s.Id) + 1,
                MedicineId = medicine.Id,
                MedicineName = medicine.FullName,
                Quantity = quantity,
                UnitPrice = medicine.Price,
                TotalAmount = medicine.Price * quantity,
                SoldOn = DateTime.Now
            };

            sales.Add(sale);
            return sale;
        });
    }
}
