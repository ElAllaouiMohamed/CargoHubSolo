using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class LoggingActionFilter : IAsyncActionFilter
{
    private readonly ILoggingService _loggingService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggingActionFilter(ILoggingService loggingService, IHttpContextAccessor httpContextAccessor)
    {
        _loggingService = loggingService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var executedContext = await next();

        if (executedContext.Exception == null &&
            (context.HttpContext.Request.Method == "POST" ||
             context.HttpContext.Request.Method == "PUT" ||
             context.HttpContext.Request.Method == "DELETE"))
        {
            var user = context.HttpContext.User.Identity?.Name ?? "anonymous";
            var endpoint = context.HttpContext.Request.Path;
            var entity = context.Controller.ToString();
            var action = context.HttpContext.Request.Method;
            var details = ""; // eventueel request body of response als string

            await _loggingService.LogAsync(user, entity, action, endpoint, details);
        }
    }
}

