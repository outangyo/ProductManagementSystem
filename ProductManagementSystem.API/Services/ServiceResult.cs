namespace ProductManagementSystem.API.Services;

public class ServiceResult<T>
{
    public bool IsSuccess { get; private init; }
    public T? Value { get; private init; }
    public string? ErrorMessage { get; private init; }
    public bool IsNotFound { get; private init; }

    public static ServiceResult<T> Success(T value) => new() 
    { 
        IsSuccess = true, 
        Value = value 
    };

    public static ServiceResult<T> Failure(string errorMessage) => new() 
    { 
        IsSuccess = false, 
        ErrorMessage = errorMessage 
    };

    public static ServiceResult<T> NotFound(string errorMessage = "Not found.") => new() 
    { 
        IsSuccess = false, 
        IsNotFound = true, 
        ErrorMessage = errorMessage 
    };
}
