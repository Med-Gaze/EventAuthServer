using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace med.common.library.model
{
    public class ErrorResponseModel
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Method { get; set; }
        public string Path { get; set; }
    }
}
