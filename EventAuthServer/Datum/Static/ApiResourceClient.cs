

using IdentityServer4.Models;
using med.common.library.configuration;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace EventAuthServer.Datum.Static
{
    /// <summary>
    /// 
    /// </summary>
    public static class ApiResourceClient
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new[]
            {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource
            {
                Name = "role",
                UserClaims = new List<string> {"role"}
            }
        };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new[]
            {
            new ApiScope("authorization", "Manage authorization"),
            new ApiScope("swagger.access", "Manage swagger"),

            new ApiScope("tanent.read", "Read Access to tanent service"),
            new ApiScope("tanent.write", "Write Access to tanent service"),

            new ApiScope("event.read", "Read Access to event service"),
            new ApiScope("event.write", "Write Access to event service"),
            new ApiScope("event.delete", "Delete Access to event service")
        };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<ApiResource> GetApiResources()
        {
            var apiResources = ConfigurationValueForStatic.Configuration.GetSection("IdentityServer:ApiResources").Get<IEnumerable<ApiResource>>();
            return apiResources;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Client> GetClients()
        {
            var apiClients = ConfigurationValueForStatic.Configuration.GetSection("IdentityServer:Clients").Get<IEnumerable<Client>>();
            return apiClients;
        }
    }
}
