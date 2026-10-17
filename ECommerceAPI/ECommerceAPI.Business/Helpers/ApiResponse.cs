namespace ECommerceAPI.Business.Helpers;

public class ApiResponse<T>
{
    public bool Success { get; set; }

    public int StatusCode { get; set; }

    public string Message { get; set; } = string.Empty;

    public T? Data { get; set; }

    public List<string>? Errors { get; set; }

    public string? TraceId { get; set; }

    public static ApiResponse<T> SuccessResponse(
        T? data,
        string message,
        int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = statusCode,
            Message = message,
            Data = data,
            Errors = null
        };
    }

    public static ApiResponse<T> SuccessResponse(
        string message,
        int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            StatusCode = statusCode,
            Message = message,
            Data = default,
            Errors = null
        };
    }

    public static ApiResponse<T> ErrorResponse(
        string message,
        int statusCode,
        List<string>? errors = null,
        string? traceId = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            StatusCode = statusCode,
            Message = message,
            Data = default,
            Errors = errors,
            TraceId = traceId
        };
    }
}