namespace BookCatalog.API.Utilities.Results;

public class Result
{
    private readonly Error? _error;

    public bool IsSuccess { get; }
    public Error Error => !IsSuccess ? _error!
                        : throw new InvalidOperationException("Cannot access error of a success result.");
    protected Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        _error = error;
    }


    public static Result Ok()
      => new(true, null);
    public static Result Fail(Error error)
      => new(false, error);

    public static implicit operator Result(Error error) => Fail(error);
}
public class Result<T> : Result
{
    private readonly T? _data;
    public T Data => IsSuccess ? _data!
                    : throw new InvalidOperationException("Cannot access value of a failed result.");
    protected Result(bool success, Error? error, T? data) : base(success, error)
    {
        _data = data;
    }
    public static Result<T> Ok(T data)
       => new(true, null, data);
    public static new Result<T> Fail(Error error)
       => new(false, error, default);

    public static implicit operator Result<T>(T data) => Ok(data);
    public static implicit operator Result<T>(Error error) => Fail(error);
}
