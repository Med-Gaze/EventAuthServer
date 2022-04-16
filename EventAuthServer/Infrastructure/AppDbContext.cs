using EventAuthServer.Entity;
using EventAuthServer.ModelBuilderExtension;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using med.common.api.library.database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EventAuthServer
{
    /// <summary>
    /// 
    /// </summary>
    public class AppDbContext :  IdentityDbContext<AppUserModel, IdentityRole<string>, string, IdentityUserClaim<string>,
            IdentityUserRole<string>,
            IdentityUserLogin<string>,
            IdentityRoleClaim<string>,
            IdentityUserToken<string>>,
        IPersistedGrantDbContext, IConfigurationDbContext
    {
        private readonly IConfiguration configuration;
        private readonly IHttpContextAccessor httpContext;
        private readonly OperationalStoreOptions operationalStoreOptions;
        private readonly ConfigurationStoreOptions configurationstoreOptions;
        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration,
            IHttpContextAccessor httpContext,
            IOptions<OperationalStoreOptions> operationalStoreOptions,
            IOptions<ConfigurationStoreOptions> configurationstoreOptions) : base(options)
        {
            this.configuration = configuration;
            this.httpContext = httpContext;
            this.operationalStoreOptions = operationalStoreOptions.Value ?? throw new ArgumentNullException(nameof(operationalStoreOptions));
            this.configurationstoreOptions = configurationstoreOptions.Value ?? throw new ArgumentNullException(nameof(configurationstoreOptions));
        }
        public DbContext Instance => this;
        public DbSet<PersistedGrant> PersistedGrants { get; set; }
        public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }
        public DbSet<OrganizationModel> Organization { get; set; }
        public DbSet<MenuModel> Menu { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientCorsOrigin> ClientCorsOrigins { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }
        public DbSet<ApiScope> ApiScopes { get; set; }
        public DbSet<FileDriver> FileDriver { get; set; }

        Task<int> IPersistedGrantDbContext.SaveChangesAsync() => base.SaveChangesAsync();
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string conn = ConnectionStringConfiguration.GetConnectionStrings(this.configuration, this.httpContext);
                optionsBuilder.UseSqlServer(conn);
            };
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            #region decimal precision

            foreach (var property in builder.Model.GetEntityTypes()
                    .SelectMany(t => t.GetProperties())
                    .Where(p => p.ClrType == typeof(decimal)))
            {
                property.SetColumnType("decimal(15, 4)");
            }

            #endregion decimal precision

            builder.ConfigurePersistedGrantContext(this.operationalStoreOptions);
            builder.ConfigureClientContext(this.configurationstoreOptions);
            builder.ConfigureResourcesContext(this.configurationstoreOptions);

            base.OnModelCreating(builder);

            ModelBuilderExtensionsHelper.DescriptionProperty(builder);

        }


    }
}
