using EventAuthServer.Datum.Static;
using EventAuthServer.Entity;
using EventAuthServer.Helper;
using IdentityServer4;
using IdentityServer4.Services;
using med.common.api.library.fileupload;
using med.common.library.configuration;
using med.common.library.configuration.service;
using med.common.library.security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using static med.common.library.constant.Policy;

namespace EventAuthServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews(options =>
            {
                options.RequireHttpsPermanent = true; // does not affect api requests
                options.RespectBrowserAcceptHeader = true;

            }).ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressConsumesConstraintForFormFileParameters = true;
                options.SuppressInferBindingSourcesForParameters = true;
                options.SuppressModelStateInvalidFilter = true;
                options.SuppressMapClientErrors = true;
                options.ClientErrorMapping[404].Link =
                    "https://httpstatuses.com/404";

            }).AddNewtonsoftJson(x =>
                            x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddDbContext<AppDbContext>().AddHealthChecks();


            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<AppUserModel, IdentityRole<string>>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 2;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromDays(1);
                options.Lockout.MaxFailedAccessAttempts = Configuration.GetSection("JWTToken:MaxFailedAccess").Get<int>();
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = true;
                options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedAccount = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
            })
                .AddRoles<IdentityRole<string>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromDays(1));


            services.Configure<PasswordHasherOptions>(option =>
            {
                option.IterationCount = 11250;
            });

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
                options.EmitStaticAudienceClaim = true;
            })
            .AddAspNetIdentity<AppUserModel>()
            .AddInMemoryIdentityResources(ApiResourceClient.GetIdentityResources())
            .AddInMemoryApiScopes(ApiResourceClient.GetApiScopes())
            .AddInMemoryApiResources(ApiResourceClient.GetApiResources())
            .AddInMemoryClients(ApiResourceClient.GetClients())
            .AddProfileService<IdentityProfileService>();

            builder.AddDeveloperSigningCredential();

           
            services.AddAuthentication(IdentityServerConstants.DefaultCookieAuthenticationScheme).AddGoogle(options =>
           {
               IConfigurationSection googleAuthNSection =
               Configuration.GetSection("IdentityConfig:SocialMedia:Google");
               options.ClientId = googleAuthNSection["ClientId"];
               options.ClientSecret = googleAuthNSection["ClientSecret"];

               options.SaveTokens = true;
           }).AddFacebook(options =>
           {
               IConfigurationSection FBAuthNSection =
               Configuration.GetSection("IdentityConfig:SocialMedia:Facebook");
               options.ClientId = FBAuthNSection["ClientId"];
               options.ClientSecret = FBAuthNSection["ClientSecret"];
           }).AddCookie().AddLocalApi();

            services.ConfigureApplicationCookie(options => {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(IdentityServerConstants.LocalApi.PolicyName, policy =>
                {
                    policy.AddAuthenticationSchemes(IdentityServerConstants.DefaultCookieAuthenticationScheme);
                    policy.RequireAuthenticatedUser();
                    // custom requirements
                });
            });

            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.AutomaticAuthentication = false;
            });

            services.Configure<IISOptions>(options =>
            {
                options.ForwardClientCertificate = false;
            });


            services.AddSwaggerGen();

            services.RegisterApiVersion();

            var HostingEnvironment = Configuration.GetSection("CorsSiteConfiguration").GetChildren().Select(x => x.Value).ToArray();

            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyParam.CorsPolicyName,
                builder =>
                {
                    builder.WithOrigins(HostingEnvironment).AllowAnyHeader().SetIsOriginAllowedToAllowWildcardSubdomains().AllowAnyMethod();
                });
            });

            services.AddHttpClient();

            services.AddHttpContextAccessor();

            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.Configure<IdentityConfig>(options => Configuration.GetSection("IdentityConfig").Bind(options));
            services.Configure<ModuleSettings>(options => Configuration.GetSection("ModuleConfig").Bind(options));
            services.Configure<CryptoSettings>(options => Configuration.GetSection("CryptoConfig").Bind(options));
            var hasAWSStorage = Configuration.GetSection("AWSConfig:Storage").Exists();
            if (hasAWSStorage)
                services.AddSingleton<IAWSFileUploader, AWSFileUploader>();
            services.AddTransient<IProfileService, IdentityProfileService>();
            services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, Swagger>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddTransient<ICryptography, Cryptography>();
            services.AddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<IEmailService, EmailService>();
            services.AddHttpClient<IConfigurationService, ConfigurationService>()
               .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                RequireHeaderSymmetry = false,
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseStaticFiles();

            #region swagger configuration 


            app.UseSwagger();

            app.UseSwaggerUI(options =>
            {
                //Build a swagger endpoint for each discovered API version  
                foreach (var description in provider.ApiVersionDescriptions.OrderByDescending(x => x.ApiVersion).ToList())
                {

                    var swaggerEndPoint = $"/swagger/{description.GroupName}/swagger.json";
                    if (env.IsStaging() || env.IsProduction())
                    {
                        swaggerEndPoint = $"/swagger/{description.GroupName}/swagger.json";
                    }
                    options.SwaggerEndpoint(swaggerEndPoint, $"Api {description.GroupName}");
                    options.RoutePrefix = "swagger";
                    options.InjectStylesheet("/swagger/ui/custom.css");
                    options.InjectJavascript("/swagger/ui/custom.js");
                    options.DocumentTitle = "API endpoint Collection";
                    options.OAuthUsePkce();
                }
            });

            #endregion

            app.UseRouting();

            app.UseCors(CorsPolicyParam.CorsPolicyName);

            app.UseIdentityServer();

            app.UseCookiePolicy();

            app.UseAuthorization();

            app.UseSerilogRequestLogging();

            app.UseApiVersioning();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();

            });
        }
    }
}
