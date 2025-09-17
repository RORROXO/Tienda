using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace tienda.src.Exceptions
{
    public class UnauthorizedAppException : Exception
    {
        public string ErrorCode { get; }
        public UnauthorizedAppException(string message, string? errorCode = "NOT_FOUND") : base(message)
            => ErrorCode = errorCode ?? "NOT_FOUND";
    }
}