using CargohubV2.Contexts;
using CargohubV2.DataConverters;
using CargohubV2.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CargoHubDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CargoHubDatabase")));

builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddScoped<ClientService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<ItemGroupService>();
builder.Services.AddScoped<ItemLineService>();
builder.Services.AddScoped<ItemTypeService>();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ShipmentService>();
builder.Services.AddScoped<SupplierService>();
builder.Services.AddScoped<TransferService>();
builder.Services.AddScoped<WarehouseService>();
builder.Services.AddScoped<ReportingService>();

builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<ApiKeyFilter>();

builder.Services.AddControllers(options =>
{
    options.Filters.AddService<ApiKeyFilter>();
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.MaxDepth = 64; // optioneel, verhoog max diepte
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

if (args.Length > 0 && args[0] == "seed")
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CargoHubDbContext>();
    DataToJSON.ImportData(context); // Import other data if needed
    DataSeeder.SeedApiKeys(context); // Seed API keys directly from here
}

app.Run();
