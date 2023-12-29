
using med.common.api.library.constant;
using med.common.library.constant;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace med.common.library.filter
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]

    public class PermissionAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string[] permissions = null;
        public PermissionAttribute(params string[] permissions)
        {
            this.permissions = permissions;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userClaims = context.HttpContext.User.Claims
                .Where(x => x.Type == CustomClaimTypes.Permission)
                .Select(a => a.Value).ToList();
            if (permissions.Length == 0)
            {
                return;
            }
            if (context.HttpContext.User.IsInRole(IdentityRoleConstant.SuperAdmin))
            {
                return;
            }
            if (userClaims.Count > 0)
            {
                bool isAuth = userClaims.Any(x => permissions.Contains(x));
                if (isAuth) return;
            }

            context.Result = new StatusCodeResult((int)System.Net.HttpStatusCode.Forbidden);
            return;
        }
    }
}
