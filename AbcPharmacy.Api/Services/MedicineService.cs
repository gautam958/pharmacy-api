using AbcPharmacy.Api.Data;
using AbcPharmacy.Api.Models;
using Microsoft.Extensions.Options;

namespace AbcPharmacy.Api.Services;

public interface IMedicineService
{
    PagedResult<MedicineDto> GetMedicines(string? search, string? filter, string? sortBy, string? sortDir, int page, int pageSize);
    Medicine GetMedicine(int id);
    Medicine AddMedicine(AddMedicineRequest request);
}

public class MedicineService : IMedicineService
{
    private readonly JsonDataStore _store;
    private readonly InventorySettings _settings;

    public MedicineService(JsonDataStore store, IOptions<InventorySettings> settings)
    {
        _store = store;
        _settings = settings.Value;
    }

    public PagedResult<MedicineDto> GetMedicines(string? search, string? filter, string? sortBy, string? sortDir, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 500);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var expiryLimit = today.AddDays(_settings.ExpiryWarningDays);

        return _store.Read((medicines, _) =>
        {
            IEnumerable<Medicine> query = medicines;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.FullName.Contains(search.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            query = filter?.ToLower() switch
            {
                "expiring" => query.Where(m => m.ExpiryDate < expiryLimit),
                "lowstock" => query.Where(m => m.Quantity < _settings.LowStockThreshold),
                _ => query
            };

            var descending = sortDir?.ToLower() == "desc";
            query = sortBy?.ToLower() switch
            {
                "brand" => descending ? query.OrderByDescending(m => m.Brand) : query.OrderBy(m => m.Brand),
                "expirydate" => descending ? query.OrderByDescending(m => m.ExpiryDate) : query.OrderBy(m => m.ExpiryDate),
                "quantity" => descending ? query.OrderByDescending(m => m.Quantity) : query.OrderBy(m => m.Quantity),
                "price" => descending ? query.OrderByDescending(m => m.Price) : query.OrderBy(m => m.Price),
                _ => descending ? query.OrderByDescending(m => m.FullName) : query.OrderBy(m => m.FullName)
            };

            var list = query.ToList();

            return new PagedResult<MedicineDto>
            {
                TotalCount = list.Count,
                Page = page,
                PageSize = pageSize,
                Items = list.Skip((page - 1) * pageSize).Take(pageSize).Select(m => ToDto(m, today)).ToList()
            };
        });
    }

    public Medicine GetMedicine(int id)
    {
        return _store.Read((medicines, _) => medicines.FirstOrDefault(m => m.Id == id))
            ?? throw new NotFoundException($"Medicine {id} not found.");
    }

    public Medicine AddMedicine(AddMedicineRequest request)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (request.ExpiryDate < today)
        {
            throw new BusinessException("Expiry date cannot be in the past.");
        }

        return _store.Write((medicines, _) =>
        {
            var name = request.FullName.Trim();
            var brand = request.Brand.Trim();

            // same name + brand + expiry is the same batch
            var exists = medicines.Any(m =>
                m.ExpiryDate == request.ExpiryDate &&
                m.FullName.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                m.Brand.Equals(brand, StringComparison.OrdinalIgnoreCase));

            if (exists)
            {
                throw new BusinessException($"{name} ({brand}) with this expiry date already exists.");
            }

            var medicine = new Medicine
            {
                Id = medicines.Count == 0 ? 1 : medicines.Max(m => m.Id) + 1,
                FullName = name,
                Brand = brand,
                Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
                ExpiryDate = request.ExpiryDate!.Value,
                Quantity = request.Quantity!.Value,
                Price = request.Price!.Value,
                CreatedOn = DateTime.Now
            };

            medicines.Add(medicine);
            return medicine;
        });
    }

    private MedicineDto ToDto(Medicine m, DateOnly today)
    {
        return new MedicineDto
        {
            Id = m.Id,
            FullName = m.FullName,
            Brand = m.Brand,
            ExpiryDate = m.ExpiryDate,
            Quantity = m.Quantity,
            Price = m.Price,
            IsExpired = m.ExpiryDate < today,
            IsExpiringSoon = m.ExpiryDate < today.AddDays(_settings.ExpiryWarningDays),
            IsLowStock = m.Quantity < _settings.LowStockThreshold
        };
    }
}
