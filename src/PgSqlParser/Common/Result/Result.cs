using System.Diagnostics;

namespace PgSqlParser.Common.Result
{
    [DebuggerDisplay("{DebuggerDisplay,nq}", Type = "Result<{typeof(T).Name},{typeof(TError).Name}>")]
    [DebuggerTypeProxy(typeof(ResultDebuggerProxy<,>))]
    public readonly struct Result<T, TError>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public T Value { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public TError Error { get; }

        private Result(bool isSuccess, T value, TError error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T, TError> Success(T value)
            => new(true, value, default!);

        public static Result<T, TError> Failure(TError error)
            => new(false, default!, error);

        public void Deconstruct(out bool isSuccess, out T value, out TError error)
            => (isSuccess, value, error) = (IsSuccess, Value, Error);

        public Result<TNext, TError> Map<TNext>(Func<T, TNext> selector)
            => IsSuccess ? Result<TNext, TError>.Success(selector(Value))
                         : Result<TNext, TError>.Failure(Error);

        public Result<TNext, TError> Bind<TNext>(Func<T, Result<TNext, TError>> binder)
            => IsSuccess ? binder(Value)
                         : Result<TNext, TError>.Failure(Error);

        public Result<TNext, TError> Select<TNext>(Func<T, TNext> selector) => Map(selector);

        public Result<TNext, TError> SelectMany<TIntermediate, TNext>(
            Func<T, Result<TIntermediate, TError>> bind,
            Func<T, TIntermediate, TNext> project)
        {
            if (!IsSuccess) return Result<TNext, TError>.Failure(Error);
            var mid = bind(Value);
            return mid.IsSuccess
                ? Result<TNext, TError>.Success(project(Value, mid.Value))
                : Result<TNext, TError>.Failure(mid.Error);
        }
        private string DebuggerDisplay
            => IsSuccess ? $"Success({Value})" : $"Failure({Error})";
    }

    internal sealed class ResultDebuggerProxy<T, TError>
    {
        private readonly Result<T, TError> _inner;

        public ResultDebuggerProxy(Result<T, TError> inner) => _inner = inner;

        public bool IsSuccess => _inner.IsSuccess;
        public bool IsFailure => _inner.IsFailure;

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private object Display =>
            _inner.IsSuccess
                ? new SuccessView(_inner.Value)
                : new FailureView(_inner.Error);

        private sealed class SuccessView
        {
            public SuccessView(T value) { Value = value; }
            public T Value { get; }
        }

        private sealed class FailureView
        {
            public FailureView(TError error) { Error = error; }
            public TError Error { get; }
        }
    }

    public static class Result
    {
        public static Result<Unit, TError> Success<TError>()
            => Result<Unit, TError>.Success(Unit.Value);

        public static Result<T, TError> Success<T, TError>(T value)
            => Result<T, TError>.Success(value);

        public static Result<T, TError> Failure<T, TError>(TError error)
            => Result<T, TError>.Failure(error);
    }
}
