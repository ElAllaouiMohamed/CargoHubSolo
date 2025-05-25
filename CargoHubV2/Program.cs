using CargohubV2.Contexts;
using CargohubV2.DataConverters;
using CargohubV2.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CargoHubDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CargoHubDatabase")));

builder.Services.AddScoped<LoggingService>();
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

builder.Services.AddControllers();
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
    DataToJSON.ImportData(context);
}

app.Run();
