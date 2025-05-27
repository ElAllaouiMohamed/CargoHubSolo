public interface ILoggingService
{
    Task LogAsync(string user, string entity, string action, string path, string message);
}

