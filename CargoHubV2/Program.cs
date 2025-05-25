using CargoHubV2.Data;
using Microsoft.EntityFrameworkCore;
using CargoHubV2.Seeding; // Of jouw juiste namespace

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CargoHubContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CargoHubContext>();
    await DatabaseJSON.SeedAsync(context);
}
