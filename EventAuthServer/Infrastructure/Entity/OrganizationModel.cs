using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventAuthServer.Entity
{
    public class OrganizationModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string LogoUrl { get; set; }
    }
}
