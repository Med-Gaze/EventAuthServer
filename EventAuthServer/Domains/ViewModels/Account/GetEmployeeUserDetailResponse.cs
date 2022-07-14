using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventAuthServer.Domains.ViewModels.Account
{
    public class GetEmployeeUserDetailResponse
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }

        public string RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
