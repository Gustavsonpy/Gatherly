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

        private Result(bool isSuccess, T? value, List<string> errors)
        {
            IsSuccess = isSuccess;
            Value = value;
            Errors = errors;
        }

        public static Result<T> Success(T value) =>
            new(true, value, new List<string>());

        public static Result<T> Failure(string error) =>
            new(false, default, new List<string> { error });

        public static Result<T> Failure(List<string> errors) =>
            new(false, default, errors);
    }
}