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
    options.JsonSerializerOptions.MaxDepth = 64; // Optioneel, verhoog max diepte
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
