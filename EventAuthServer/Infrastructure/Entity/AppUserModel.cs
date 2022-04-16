using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventAuthServer.Entity
{
    public class AppUserModel : IdentityUser
    {
        public string FullName { get; set; }
        public string NickName { get; set; }
        public Guid? FileId { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
