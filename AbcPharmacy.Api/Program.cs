using AbcPharmacy.Api.Data;
using AbcPharmacy.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.Configure<InventorySettings>(builder.Configuration.GetSection("Inventory"));
builder.Services.AddSingleton<JsonDataStore>();
builder.Services.AddScoped<IMedicineService, MedicineService>();
builder.Services.AddScoped<ISaleService, SaleService>();



// Angular app runs on a different port, so allow it to call the api
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

// Add RateLimiting Logic

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.MapControllers();

// load the json files at startup instead of on the first request
app.Services.GetRequiredService<JsonDataStore>();

app.Run();
