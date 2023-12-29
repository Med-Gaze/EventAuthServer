using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace med.common.library.configuration
{
    public static class ConfigurationValueForStatic
    {
        private static IConfiguration config;
        public static IConfiguration Configuration
        {
            get
            {
                string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddJsonFile(path: $"appsettings.{env}.json", optional: true, reloadOnChange: true);
                config = builder.Build();
                return config;
            }
        }
    }
}
