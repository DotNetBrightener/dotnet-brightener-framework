namespace LanguageExts.Results;

/// <summary>
///     Represents the result of an operation.
///     In case of success, the value is returned, otherwise, an <see cref="Exception"/> is returned.
/// </summary>
/// <typeparam name="TValue">The bound value type</typeparam>
public class Result<TValue> : Result<TValue, Exception>
{
    public Result(TValue value)
        : base(value)
    {
    }

    public Result(Exception error)
    : base(error)
    {
    }

    public static implicit operator Result<TValue>(TValue value) => new(value);

    public static implicit operator Result<TValue>(Exception error) => new(error);
}

/// <summary>
///     Represents the result of an operation.
///     In case of success, the value is returned, otherwise, an instance of type <typeparamref name="TError"/> is returned.
/// </summary>
/// <typeparam name="TValue">The bound value type</typeparam>
/// <typeparam name="TError">The bound error type</typeparam>
public class Result<TValue, TError>
{
    private readonly TValue _value;
    private readonly TError _error;

    public Result(TValue value)
    {
        _value = value;
        _error = default;
    }

    public Result(TError error)
    {
        _error = error;
        _value = default;
    }


    public bool IsSuccess => _error == null;

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);

    public static implicit operator Result<TValue, TError>(TError error) => new(error);

    public TValue IfFail(TValue defaultValue) => !IsSuccess ? defaultValue : _value!;

    public TValue IfSuccess(Action<TValue> action)
    {
        if (IsSuccess)
        {
            action(_value!);

            return _value!;
        }

        return default;
    }

    public TResult Match<TResult>(Func<TValue, TResult> success,
                                  Func<TError, TResult> failure) => IsSuccess ? success(_value!) : failure(_error!);

    public TValue Value => IsSuccess ? _value : default;

    public TError Error => !IsSuccess ? _error : default;
}