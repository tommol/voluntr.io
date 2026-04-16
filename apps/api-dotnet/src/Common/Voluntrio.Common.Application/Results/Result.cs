namespace Voluntrio.Common.Application.Results;

/// <summary>
/// Represents the result of an operation that may succeed or fail.
/// </summary>
public class Result
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Result"/> class.
    /// </summary>
    /// <param name="isSuccess">Indicates whether the operation was successful.</param>
    /// <param name="error">The error that occurred, if any.</param>
    /// <exception cref="ArgumentException">Thrown when the success state and error state are inconsistent.</exception>
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new ArgumentException("Successful result cannot have an error.", nameof(error));
        }

        if (!isSuccess && error == Error.None)
        {
            throw new ArgumentException("Failure result must include an error.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error associated with a failed operation.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <returns>A successful result with no error.</returns>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    /// <returns>A failed result with the specified error.</returns>
    public static Result Failure(Error error) => new(false, error);
}

/// <summary>
/// Represents the result of an operation that may succeed with a value or fail.
/// </summary>
/// <typeparam name="TValue">The type of value returned on success.</typeparam>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class with a success value.
    /// </summary>
    /// <param name="value">The successful operation result value.</param>
    private Result(TValue value)
        : base(true, Error.None)
    {
        _value = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Result{TValue}"/> class with a failure error.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    private Result(Error error)
        : base(false, error)
    {
        _value = default;
    }

    /// <summary>
    /// Gets the value from a successful result.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the result is a failure.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No value available for failed result.");

    /// <summary>
    /// Creates a successful result with the specified value.
    /// </summary>
    /// <param name="value">The successful operation result value.</param>
    /// <returns>A successful result containing the value.</returns>
    public static Result<TValue> Success(TValue value) => new(value);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    /// <returns>A failed result with the specified error.</returns>
    public static new Result<TValue> Failure(Error error) => new(error);
}
