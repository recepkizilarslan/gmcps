namespace Gmcps.Domain;

public interface IGmcpsResult
{
    bool IsSuccess { get; }

    bool IsFailure { get; }

    object ValueObject { get; }

    string Error { get; }
}

public sealed class Result<T> : IGmcpsResult
{
    private readonly T? _value;

    private readonly string? _error;

    private Result(T value)
    {
        _value = value;
        IsSuccess = true;
    }

    private Result(string error)
    {
        _error = error;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot access Value on a failed Result. Error: {_error}");

    public string Error => IsFailure
        ? _error!
        : throw new InvalidOperationException("Cannot access Error on a successful Result.");

    object IGmcpsResult.ValueObject => Value!;

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(string error) => new(error);

    public Result<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        IsSuccess ? Result<TOut>.Success(mapper(_value!)) : Result<TOut>.Failure(_error!);

    public async Task<Result<TOut>> MapAsync<TOut>(Func<T, Task<TOut>> mapper) =>
        IsSuccess ? Result<TOut>.Success(await mapper(_value!)) : Result<TOut>.Failure(_error!);

    public Result<TOut> Bind<TOut>(Func<T, Result<TOut>> binder) =>
        IsSuccess ? binder(_value!) : Result<TOut>.Failure(_error!);

    public T GetValueOrDefault(T defaultValue) => IsSuccess ? _value! : defaultValue;
}
