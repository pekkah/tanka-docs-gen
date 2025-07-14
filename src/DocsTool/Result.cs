using System;
using System.Diagnostics.CodeAnalysis;

namespace Tanka.DocsTool
{
    public readonly struct Result<T>
    {
        private readonly T _value;
        private readonly string? _error;

        private Result(T value)
        {
            _value = value;
            _error = null;
            IsSuccess = true;
        }

        private Result(string error)
        {
            _value = default!;
            _error = error;
            IsSuccess = false;
        }

        public bool IsSuccess { get; }

        public bool IsFailure => !IsSuccess;

        public T Value => IsSuccess ? _value : throw new InvalidOperationException("Result does not have a value.");

        public string Error => IsFailure ? _error! : throw new InvalidOperationException("Result does not have an error.");

        public static implicit operator Result<T>(T value) => new Result<T>(value);
        public static implicit operator Result<T>(string error) => new Result<T>(error);

        public static Result<T> Success(T value) => new(value);
        public static Result<T> Failure(string error) => new(error);

        public T GetValueOrDefault(T defaultValue) => IsSuccess ? _value : defaultValue;
    }

    public static class Result
    {
        public static Result<T> Success<T>(T value) => Result<T>.Success(value);
        public static Result<T> Failure<T>(string error) => Result<T>.Failure(error);
    }
}