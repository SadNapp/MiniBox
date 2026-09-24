namespace MiniBox.Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException();

        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
}

public class Result<TValue> : Result
{
    private readonly TValue? _value;

    public TValue Value => IsSuccess 
        ? _value! 
        : throw new InvalidOperationException("Неможливо отримати значення для Result.Failure.");

    protected Result(TValue? value, bool isSuccess, string error) 
        : base(isSuccess, error)
    {
        _value = value;
    }

    public static Result<TValue> Success(TValue value) => new(value, true, string.Empty);
    public static new Result<TValue> Failure(string error) => new(default, false, error);
}