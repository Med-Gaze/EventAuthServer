using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace med.common.library.exception
{
    public class ApiException : Exception
    {
        public HttpStatusCode Status { get; private set; }

        public ApiException(HttpStatusCode status, string msg) : base(msg)
        {
            Status = status;
        }
        public ApiException()
           : base()
        {
        }

        public ApiException(string message)
            : base(message)
        {
        }

        public ApiException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ApiException(string name, object key)
            : base($"Entity \"{name}\" ({key}) was not found.")
        {
        }
    }
}
