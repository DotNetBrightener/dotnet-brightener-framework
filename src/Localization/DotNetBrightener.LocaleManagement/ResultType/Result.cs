#nullable enable
using System;

namespace DotNetBrightener.LocaleManagement.ResultType;

public readonly struct Result<TValue, TError>
{
    private readonly TValue? _value;
    private readonly TError? _error;

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

    public TResult Match<TResult>(Func<TValue, TResult> success,
                                  Func<TError, TResult> failure) => IsSuccess ? success(_value!) : failure(_error!);

    public TValue? Value => IsSuccess ? _value : default;

    public TError? Error => !IsSuccess ? _error : default;
}