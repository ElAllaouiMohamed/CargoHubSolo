using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

public class ApiKeyFilter : IAsyncActionFilter
{
    private readonly IApiKeyService _apiKeyService;
    private readonly ILogger<ApiKeyFilter> _logger;

    public ApiKeyFilter(IApiKeyService apiKeyService, ILogger<ApiKeyFilter> logger)
    {
        _apiKeyService = apiKeyService;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var httpContext = context.HttpContext;

        if (!httpContext.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey))
        {
            httpContext.Response.StatusCode = 401;
            await httpContext.Response.WriteAsync("API Key is missing.");
            return;
        }

        string providedKeyHash = HashKey(extractedApiKey);

        var adminKeyHash = await _apiKeyService.GetKeyHashByNameAsync("Admin");

        if (providedKeyHash == adminKeyHash)
        {
            _logger.LogInformation("API key goedgekeurd.");
            await next();
        }
        else
        {
            httpContext.Response.StatusCode = 403;
            await httpContext.Response.WriteAsync("unauthorized client.");
        }
    }

    private static string HashKey(string key)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
        return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
    }
}

