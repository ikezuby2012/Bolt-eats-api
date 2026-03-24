using System.Net;

namespace SharedKernel;

public enum ResponseStatus
{
    success,
    error,
    warning
}

public class ApiResponse<T>
{
    public string Status { get; set; }
    public int Code { get; set; }
    public string Message { get; set; }
    public T? Data { get; set; }

    public ApiResponse() { }

    protected internal ApiResponse(string status, int code, string message, T? data)
    {
        Status = status;
        Code = code;
        Message = message;
        Data = data;
    }

    public static ApiResponse<T> Success(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>(ResponseStatus.success.ToString(), (int)HttpStatusCode.OK, message, data);
    }

    public static ApiResponse<T> Error(string message, int code = (int)HttpStatusCode.InternalServerError)
    {
        return new ApiResponse<T>(ResponseStatus.error.ToString(), code, message, default!);
    }

    public static ApiResponse<T> Warning(string message, T? data)
    {
        return new ApiResponse<T>(ResponseStatus.warning.ToString(), (int)HttpStatusCode.OK, message, data);
    }
}

public class ApiResponse : ApiResponse<object>
{
    private ApiResponse() { }

    public static new ApiResponse<object> Error(string message, int code = (int)HttpStatusCode.InternalServerError)
    {
        return ApiResponse<object>.Error(message, code);
    }

    public static new ApiResponse<object> Success(object data, string message = "Operation successful")
    {
        return ApiResponse<object>.Success(data, message);
    }
}
