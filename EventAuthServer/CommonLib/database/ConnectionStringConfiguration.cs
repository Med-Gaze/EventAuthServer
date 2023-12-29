using med.common.library.constant;
using med.common.library.extension;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace med.common.api.library.database
{
    public static class ConnectionStringConfiguration
    {
        public static string GetConnectionStrings(IConfiguration configuration, IHttpContextAccessor httpContext)
        {
            var hasAcceptTenant = configuration.GetSection("DbConfig").GetValue<bool>("Tanent");
            if (hasAcceptTenant)
            {
                var val = httpContext.HttpContext?.Request?.Headers[Common.RequestDomain];
                var host = httpContext.HttpContext?.Request?.Host.Host;
                var subDomain = string.Empty;

                // from host url
                if (!string.IsNullOrWhiteSpace(host) && (!val.HasValue || val.Value.Count == 0))
                    subDomain = host.Split('.')[0].Trim().ToLower() ?? string.Empty;

                //from tanent request
                if (val.HasValue && val.Value.Count > 0)
                    subDomain = val.Value.SingleOrDefault().Split(',')[0].Trim().ToString() ?? val.Value.SingleOrDefault().ToString();

                //manage connection string
                if (!string.IsNullOrEmpty(subDomain))
                {
                    var cloudTanentServer = string.Format(configuration.GetConnectionString("CloudTanentServer"), subDomain.ToTitleCase());
                    return cloudTanentServer;
                }
                else
                {
                    return configuration.GetConnectionString("SQLConnection");
                }
            }

            return configuration.GetConnectionString("SQLConnection");
        }

    }
}
