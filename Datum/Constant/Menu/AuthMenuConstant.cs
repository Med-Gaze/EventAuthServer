using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventAuthServer.Datum.Constant.Menu
{
    public static class AuthMenuConstant
    {
        public static KeyValuePair<string, string> Company = new("Organization Setting", "orgmanagement");
        public static KeyValuePair<string, string> Role = new("Role Setting", "rolemanagement");
        public static KeyValuePair<string, string> Menu = new("Menu Setting", "menumanagement");
        public static KeyValuePair<string, string> User = new("User Setting", "usermanagement");
    }
}
