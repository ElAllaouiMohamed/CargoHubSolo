using System.Threading.Tasks;
using CargohubV2.Contexts;
using Microsoft.EntityFrameworkCore;

public class ApiKeyService : IApiKeyService
{
    private readonly CargoHubDbContext _context;

    public ApiKeyService(CargoHubDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetKeyHashByNameAsync(string name)
    {
        var key = await _context.ApiKeys.FirstOrDefaultAsync(k => k.Name == name);
        return key?.KeyHash;
    }
}


public interface IApiKeyService
{
    Task<string?> GetKeyHashByNameAsync(string name);
}
