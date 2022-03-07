using System;
using System.Collections.Generic;
using System.Text;

namespace EventAuthServer.Datum.Enum
{
    public enum AccountStatusEnum
    {
        Register = 0,
        Requested = 1,
        Pending = 2,
        Verified = 3,
        PasswordPending = 4
       
    }
}
