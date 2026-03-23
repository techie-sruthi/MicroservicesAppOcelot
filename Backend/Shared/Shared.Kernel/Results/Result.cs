namespace Shared.Kernel.Results;

public sealed class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? Message { get; } = null;

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    private Result(bool isSuccess, string? error, string? message)
    {
        IsSuccess = isSuccess;
        Error = error;
        Message = message;
    }

    public static Result Success(string message) => new(true, null, message);

    public static Result Failure(string error) => new(false, error);
}

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }
    public string? Message { get; }

    private Result(bool isSuccess, T? value, string? message, string? error = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Message = message;
        Error = error;
    }

    public static Result<T> Success(T value, string message) => new(true, value, message);

    public static Result<T> Failure(string error) => new(false, default, error);
}
