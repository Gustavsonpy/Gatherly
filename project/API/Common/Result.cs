using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public List<string> Errors { get; }
        public Dictionary<string, List<string>> FieldErrors { get; }

        private Result(bool isSuccess, T? value, List<string> errors, Dictionary<string, List<string>>? fieldErrors = null)
        {
            IsSuccess = isSuccess;
            Value = value;
            Errors = errors;
            FieldErrors = fieldErrors ?? new Dictionary<string, List<string>>();
        }

        public static Result<T> Success(T value) =>
            new(true, value, new List<string>());

        public static Result<T> Failure(string error) =>
            new(false, default, new List<string> { error });

        public static Result<T> Failure(List<string> errors) =>
            new(false, default, errors);

        public static Result<T> Failure(Dictionary<string, List<string>> fieldErrors) =>
            new(false, default, fieldErrors.SelectMany(kv => kv.Value).ToList(), fieldErrors);
    }
}