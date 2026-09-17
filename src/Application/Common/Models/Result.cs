using ERP_Government.Application.Common.Errors;

namespace ERP_Government.Application.Common.Models;

public class Result
{
    internal Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    internal Result(bool succeeded, string? code, ErrorCategory? category, string? message, string? target, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Code = code;
        Category = category;
        Message = message;
        Target = target;
        Errors = errors.ToArray();
    }

    public bool Succeeded { get; init; }

    public string[] Errors { get; init; }

    public string? Code { get; init; }

    public ErrorCategory? Category { get; init; }

    public string? Message { get; init; }

    public string? Target { get; init; }

    public static Result Success()
    {
        return new Result(true, Array.Empty<string>());
    }

    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors);
    }

    public static Result Failure(string code, ErrorCategory category, string message, string? target = null)
    {
        return new Result(false, code, category, message, target, Array.Empty<string>());
    }
}

public class Result<T>
{
    internal Result(bool succeeded, T? value, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Value = value;
        Errors = errors.ToArray();
    }

    internal Result(bool succeeded, T? value, string? code, ErrorCategory? category, string? message, string? target, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Value = value;
        Code = code;
        Category = category;
        Message = message;
        Target = target;
        Errors = errors.ToArray();
    }

    public bool Succeeded { get; init; }
    public T? Value { get; init; }
    public string[] Errors { get; init; }

    public string? Code { get; init; }

    public ErrorCategory? Category { get; init; }

    public string? Message { get; init; }

    public string? Target { get; init; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, Array.Empty<string>());
    }

    public static Result<T> Failure(IEnumerable<string> errors)
    {
        return new Result<T>(false, default, errors);
    }

    public static Result<T> Failure(string code, ErrorCategory category, string message, string? target = null)
    {
        return new Result<T>(false, default, code, category, message, target, Array.Empty<string>());
    }
}
