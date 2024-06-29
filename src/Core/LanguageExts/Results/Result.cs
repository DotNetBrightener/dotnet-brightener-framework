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

    public static implicit operator TValue(Result<TValue> result) => result.IfFail(default);
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

    /// <summary>
    ///     Indicates if the result object is success.
    /// </summary>
    public bool IsSuccess => _error == null;

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);

    public static implicit operator Result<TValue, TError>(TError error) => new(error);

    public static implicit operator TValue(Result<TValue, TError> result) => result.IfFail(default);

    /// <summary>
    ///     Returns a value of type <typeparamref name="TValue"/> as a fallback value if the result is failure.
    /// </summary>
    /// <param name="fallbackValue">
    ///     The fallback value
    /// </param>
    /// <returns>
    ///     The value of type <typeparamref name="TValue"/> if the result is success. Otherwise, the <see cref="fallbackValue"/>.
    /// </returns>
    public TValue IfFail(TValue fallbackValue) => !IsSuccess ? fallbackValue : _value!;

    /// <summary>
    ///     Performs an action if the result is a success before return the success value.
    /// </summary>
    /// <param name="action">
    ///     The action to perform
    /// </param>
    /// <returns>
    ///     The success value if the result is success. Otherwise, <c>null</c>
    /// </returns>
    public TValue IfSuccess(Action<TValue> action)
    {
        if (IsSuccess)
        {
            action(_value!);

            return _value!;
        }

        return default;
    }

    /// <summary>
    ///     Retrieves the <typeparamref name="TResult"/> value depends on the result state.
    /// </summary>
    /// <typeparam name="TResult">
    /// </typeparam>
    /// <param name="onSuccess">
    ///     Processes the success result to return the <typeparamref name="TResult"/> object.
    /// </param>
    /// <param name="onFailure">
    ///     Processes the error result to return the <typeparamref name="TResult"/> object.
    /// </param>
    /// <returns>
    ///     The <typeparamref name="TResult"/> object
    /// </returns>
    public TResult Match<TResult>(Func<TValue, TResult> onSuccess,
                                  Func<TError, TResult> onFailure) =>
        IsSuccess ? onSuccess(_value!) : onFailure(_error!);

    public TValue Value => IsSuccess ? _value : default;

    public TError Error => !IsSuccess ? _error : default;
}