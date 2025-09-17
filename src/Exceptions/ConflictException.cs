using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tienda.src.Exceptions
{
    public class ConflictException : Exception
    {
        public string ErrorCode { get; }
        public ConflictException(string message, string? errorCode = "NOT_FOUND") : base(message)
            => ErrorCode = errorCode ?? "NOT_FOUND";
    }
}