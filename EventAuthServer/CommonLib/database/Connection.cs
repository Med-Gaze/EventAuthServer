using med.common.library.configuration;
using med.common.library.constant;
using med.common.library.model;
using med.common.library.security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace med.common.library.database
{
    public interface IDatabaseConnectionFactory
    {
        Task<IDbConnection> CreateConnection();
    }
    public class DatabaseConnectionFactory : IDatabaseConnectionFactory
    {
        private readonly IConfiguration config;
        private readonly IHttpContextAccessor httpContext;
        private readonly ModuleSettings settings;

        public DatabaseConnectionFactory(IConfiguration config,
            IHttpContextAccessor httpContext,
            IOptions<ModuleSettings> settings)
        {
            this.config = config;
            this.httpContext = httpContext;
            this.settings = settings.Value;
        }

        async Task<IDbConnection> IDatabaseConnectionFactory.CreateConnection()
        {
            try
            {
                string connectionString = this.config["ConnectionStrings:SQLConnection"];
                var hasAcceptTenant = this.config.GetSection("DbConfig").GetValue<bool>("Tanent");
                if (hasAcceptTenant)
                {
                    var host = this.httpContext.HttpContext.Request.Host.Host;
                    var masterUrl = new Uri(this.settings.BaseUrl);
                    var IsMaster = masterUrl.Host == host;
                    if (!IsMaster)
                    {
                        var requestSubDomain = this.httpContext.HttpContext.Request.Headers[Common.RequestDomain].ToString();
                        var subDomain = string.IsNullOrEmpty(requestSubDomain) ? host.Split('.').FirstOrDefault() : requestSubDomain;
                        var moduleAlias = this.config.GetSection("ModuleConfig:Alias").Value;
                        connectionString = string.Format(this.config.GetConnectionString("CloudTanentServer"), string.Concat(subDomain, moduleAlias.ToUpperInvariant(), "Db"));
                    }
                }
                var sqlConnection = new SqlConnection(connectionString);
                await sqlConnection.OpenAsync();
                return sqlConnection;
            }
            catch
            {
                throw new Exception("Database server not connected");
            }

        }
    }
}
