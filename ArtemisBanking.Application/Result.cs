namespace ArtemisBanking.Application
{
    public class Result
    {

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;

        public string? GeneralError { get; }
        public List<string>? Errors { get; }

        protected Result(bool isSuccess, string? generalError = null, List<string>? errors = null)
        {
            IsSuccess = isSuccess;
            GeneralError = generalError;
            Errors = errors;
        }

        public static Result Ok() => new Result(true);
        public static Result Fail(List<string> errors) => new Result(false, null, errors);
        public static Result Fail(string generalError) => new Result(false, generalError);
    }

    public class Result<T> : Result
    {
        public T? Value { get; }

        protected Result(bool isSuccess, T? value = default, string? generalError = null, List<string>? errors = null)
            : base(isSuccess, generalError, errors)
        {
            Value = value;
        }

        public static Result<T> Ok(T value) => new Result<T>(true, value);
        public new static Result<T> Fail(List<string> errors) => new Result<T>(false, default, null, errors);
        public new static Result<T> Fail(string generalError) => new Result<T>(false, default, generalError);
    }
}

