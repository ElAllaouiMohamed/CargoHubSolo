using CargohubV2.Contexts;
using CargohubV2.DataConverters;
using CargohubV2.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.IO;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CargoHubDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CargoHubDatabase")));

builder.Services.AddScoped<ILoggingService, LoggingService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IInventoryService, InventoryService>();
builder.Services.AddScoped<IItemService, ItemService>();
builder.Services.AddScoped<IItemGroupService, ItemGroupService>();
builder.Services.AddScoped<IItemLineService, ItemLineService>();
builder.Services.AddScoped<IItemTypeService, ItemTypeService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<ITransferService, TransferService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<ApiKeyFilter>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<LoggingActionFilter>();
    options.Filters.AddService<ApiKeyFilter>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.MaxDepth = 64;
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CargoHub API",
        Version = "v2",
        Description = "API documentatie voor CargoHub project"
    });


    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "Vul API Key.'",
        In = ParameterLocation.Header,
        Name = "X-Api-Key",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v2/swagger.json", "CargoHub API V2");
    c.RoutePrefix = string.Empty;
});

app.MapControllers();

if (args.Length > 0 && args[0] == "seed")
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<CargoHubDbContext>();
    DataToJSON.ImportData(context);
    DataSeeder.SeedApiKeys(context);
}

app.Run();
