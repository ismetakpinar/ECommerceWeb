namespace ECommerce.Web.Models;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }

    public int StatusCode { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public List<string>? Errors { get; init; }

    public string? TraceId { get; init; }
}