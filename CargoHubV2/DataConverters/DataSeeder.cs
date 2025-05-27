using CargohubV2.Models;
using CargohubV2.Contexts;
using System.Security.Cryptography;
using System.Text;

public static class DataSeeder
{
    public static void SeedApiKeys(CargoHubDbContext context)
    {
        if (!context.ApiKeys.Any())
        {
            var adminKey = Environment.GetEnvironmentVariable("AdminApiKey");
            var employeeKey = Environment.GetEnvironmentVariable("EmployeeApiKey");

            if (!string.IsNullOrEmpty(adminKey))
            {
                context.ApiKeys.Add(new ApiKey
                {
                    Name = "Admin",
                    KeyHash = HashKey(adminKey)
                });
            }
            if (!string.IsNullOrEmpty(employeeKey))
            {
                context.ApiKeys.Add(new ApiKey
                {
                    Name = "Employee",
                    KeyHash = HashKey(employeeKey)
                });
            }
            context.SaveChanges();
        }
    }

    private static string HashKey(string key)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }
}

