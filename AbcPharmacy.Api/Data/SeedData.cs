using AbcPharmacy.Api.Models;

namespace AbcPharmacy.Api.Data;

public static class SeedData
{
    private static readonly string[] Tablets =
    {
        "Paracetamol", "Ibuprofen", "Amoxicillin", "Azithromycin", "Cetirizine", "Metformin", "Amlodipine",
        "Atorvastatin", "Omeprazole", "Pantoprazole", "Losartan", "Telmisartan", "Ciprofloxacin", "Doxycycline",
        "Montelukast", "Diclofenac", "Aceclofenac", "Domperidone", "Ondansetron", "Metronidazole", "Fluconazole",
        "Aspirin", "Clopidogrel", "Rosuvastatin", "Glimepiride", "Levothyroxine", "Vitamin D3", "Vitamin B12",
        "Folic Acid", "Calcium Carbonate", "Cefixime", "Levofloxacin", "Tramadol", "Pregabalin", "Sertraline",
        "Escitalopram", "Metoprolol", "Atenolol", "Ramipril", "Furosemide", "Esomeprazole", "Rabeprazole",
        "Famotidine", "Albendazole", "Acyclovir", "Fexofenadine", "Loratadine", "Allopurinol", "Melatonin"
    };

    private static readonly string[] Syrups =
    {
        "Paracetamol", "Ibuprofen", "Amoxicillin", "Azithromycin", "Cetirizine", "Ambroxol", "Dextromethorphan",
        "Lactulose", "Sucralfate", "Multivitamin", "Zinc", "Albendazole", "Domperidone"
    };

    private static readonly string[] Creams =
    {
        "Clotrimazole", "Mupirocin", "Betamethasone", "Hydrocortisone", "Ketoconazole", "Diclofenac", "Fusidic Acid",
        "Mometasone", "Acyclovir", "Terbinafine"
    };

    private static readonly string[] Injections = { "Ceftriaxone", "Insulin Glargine", "Enoxaparin", "Pantoprazole", "Ondansetron" };

    private static readonly string[] Brands =
    {
        "Cipla", "Sun Pharma", "Dr. Reddy's", "Lupin", "Zydus", "Mankind", "Alkem", "Torrent", "Glenmark",
        "Abbott", "Pfizer", "GSK", "Sanofi", "Intas", "Micro Labs"
    };

    private static readonly string?[] Notes =
    {
        null, null, null, "Store in a cool and dry place", "Prescription required", "Keep out of reach of children",
        "Keep in refrigerator", "Take after food", "Shake well before use"
    };

    public static List<Medicine> CreateMedicines(int count)
    {
        // fixed seed so every fresh setup gets the same list
        var random = new Random(2026);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var medicines = new List<Medicine>();
        var used = new HashSet<string>();

        while (medicines.Count < count)
        {
            var (name, minPrice, maxPrice) = RandomName(random);
            var brand = Brands[random.Next(Brands.Length)];
            var expiry = RandomExpiry(random, today);

            if (!used.Add($"{name}|{brand}|{expiry}"))
            {
                continue; 
            }

            // around 10% of the items are low on stock
            var quantity = random.Next(100) < 10 ? random.Next(0, 10) : random.Next(10, 500);
            var price = Math.Round(minPrice + (decimal)random.NextDouble() * (maxPrice - minPrice), 2);

            medicines.Add(new Medicine
            {
                Id = medicines.Count + 1,
                FullName = name,
                Brand = brand,
                ExpiryDate = expiry,
                Quantity = quantity,
                Price = price,
                Notes = Notes[random.Next(Notes.Length)],
                CreatedOn = DateTime.Now.AddDays(-random.Next(1, 365))
            });
        }

        return medicines;
    }

    public static List<Sale> CreateSales(List<Medicine> medicines, int count)
    {
        var random = new Random(7);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var sellable = medicines.Where(m => m.ExpiryDate >= today).ToList();
        var sales = new List<Sale>();

        for (var i = 1; i <= count; i++)
        {
            var medicine = sellable[random.Next(sellable.Count)];
            var quantity = random.Next(1, 6);

            sales.Add(new Sale
            {
                Id = i,
                MedicineId = medicine.Id,
                MedicineName = medicine.FullName,
                Quantity = quantity,
                UnitPrice = medicine.Price,
                TotalAmount = medicine.Price * quantity,
                SoldOn = DateTime.Now.AddDays(-random.Next(0, 60)).AddMinutes(-random.Next(0, 600))
            });
        }

        return sales.OrderBy(s => s.SoldOn).ToList();
    }

    private static (string Name, decimal MinPrice, decimal MaxPrice) RandomName(Random random)
    {
        switch (random.Next(10))
        {
            case 0:
            case 1:
                return ($"{Syrups[random.Next(Syrups.Length)]} Syrup 100 ml", 45, 280);
            case 2:
                return ($"{Creams[random.Next(Creams.Length)]} Cream 30 g", 60, 350);
            case 3:
                return ($"{Injections[random.Next(Injections.Length)]} Injection", 80, 1500);
            default:
                var form = random.Next(2) == 0 ? "Tablets" : "Capsules";
                var strip = new[] { 10, 15, 20, 30 }[random.Next(4)];
                return ($"{Tablets[random.Next(Tablets.Length)]} {form} (Strip of {strip})", 20, 450);
        }
    }

    private static DateOnly RandomExpiry(Random random, DateOnly today)
    {
        var roll = random.Next(100);

        if (roll < 3)
        {
            return today.AddDays(-random.Next(1, 90)); // already expired
        }

        if (roll < 12)
        {
            return today.AddDays(random.Next(0, 30)); // expires within 30 days
        }

        return today.AddDays(random.Next(30, 900));
    }
}
