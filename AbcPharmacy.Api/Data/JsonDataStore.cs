using System.Text.Json;
using AbcPharmacy.Api.Models;

namespace AbcPharmacy.Api.Data;

// Keeps medicines and sales in memory and writes them to two json files on every change.
// Registered as a singleton, all access goes through the lock so two sales can't oversell the same stock.
public class JsonDataStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    }; 

    private readonly object _lock = new();
    private readonly string _medicinesFile;
    private readonly string _salesFile;
    private readonly List<Medicine> _medicines;
    private readonly List<Sale> _sales;

    public JsonDataStore(IWebHostEnvironment env, IConfiguration config, ILogger<JsonDataStore> logger)
    {
        var folder = Path.Combine(env.ContentRootPath, config["DataFolder"] ?? "App_Data");
        Directory.CreateDirectory(folder);

        _medicinesFile = Path.Combine(folder, "medicines.json");
        _salesFile = Path.Combine(folder, "sales.json");

        if (File.Exists(_medicinesFile))
        {
            _medicines = ReadFile<Medicine>(_medicinesFile);
            _sales = File.Exists(_salesFile) ? ReadFile<Sale>(_salesFile) : new List<Sale>();
        }
        else
        {
            // first run - create some sample data so the screens are not empty
            _medicines = SeedData.CreateMedicines(1000);
            _sales = SeedData.CreateSales(_medicines, 250);
            Save();
            logger.LogInformation("Created sample data in {Folder}", folder);
        }

        logger.LogInformation("Loaded {Medicines} medicines and {Sales} sales", _medicines.Count, _sales.Count);
    }

    public T Read<T>(Func<List<Medicine>, List<Sale>, T> query)
    {
        lock (_lock)
        {
            return query(_medicines, _sales);
        }
    }

    public T Write<T>(Func<List<Medicine>, List<Sale>, T> change)
    {
        lock (_lock)
        {
            var result = change(_medicines, _sales);
            Save();
            return result;
        }
    }

    private void Save()
    {
        WriteFile(_medicinesFile, _medicines);
        WriteFile(_salesFile, _sales);
    }

    private static List<T> ReadFile<T>(string path)
    {
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ?? new List<T>();
    }

    private static void WriteFile<T>(string path, List<T> items)
    {
        // write to a temp file first and then replace, so a crash never leaves half a file
        var tempPath = path + ".tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(items, JsonOptions));
        File.Move(tempPath, path, true);
    }
}
