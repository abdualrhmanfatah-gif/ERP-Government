namespace ERP_Government.Application.Common.Models;

public class Result
{
    internal Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    public bool Succeeded { get; init; }

    public string[] Errors { get; init; }

    public static Result Success()
    {
        return new Result(true, Array.Empty<string>());
    }

    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors);
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

    public bool Succeeded { get; init; }
    public T? Value { get; init; }
    public string[] Errors { get; init; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, Array.Empty<string>());
    }

    public static Result<T> Failure(IEnumerable<string> errors)
    {
        return new Result<T>(false, default, errors);
    }
}
