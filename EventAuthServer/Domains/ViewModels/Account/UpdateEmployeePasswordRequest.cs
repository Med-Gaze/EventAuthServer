using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventAuthServer.Domains.ViewModels.Account
{
    public class UpdateEmployeePasswordRequest
    {
        public string Id { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string NewConfirmPassword { get; set; }
    }
}
