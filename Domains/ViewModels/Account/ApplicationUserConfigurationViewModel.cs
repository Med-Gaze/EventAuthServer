using System;
using System.Collections.Generic;
using System.Text;

namespace EventAuthServer.Domains.ViewModels.Identity
{
    public class ApplicationUserConfigurationViewModel
    {
        public string Id { get; set; }
        public bool? IsDeactivated { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsBlocked { get; set; }
        public bool? IsApprove { get; set; }

    }
}
