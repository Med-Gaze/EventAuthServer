using EventAuthServer;
using EventAuthServer.Datum.Static;
using EventAuthServer.Entity;
using EventAuthServer.Helper;
using EventAuthServer.ModelBuilderExtension;
using IdentityServer4;
using IdentityServer4.Services;
using med.common.api.library.fileupload;
using med.common.library.configuration;
using med.common.library.configuration.service;
using med.common.library.security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using Serilog.Sinks.Elasticsearch;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using static med.common.library.constant.Policy;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{environment}.json",
        optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

// Create the Serilog logger, and configure the sinks

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .Enrich.WithExceptionDetails()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .Enrich.WithMachineName()
    .WriteTo.Console()
    .WriteTo.Elasticsearch(ConfigureElasticSink(config, environment))
    .CreateBootstrapLogger();


Log.Information("Starting host");
try
{
    var builder = WebApplication.CreateBuilder(args);
    // Add services to the container.
    builder.WebHost.UseContentRoot(Directory.GetCurrentDirectory())
        .UseUrls("http://*:44321").CaptureStartupErrors(true)
        .ConfigureAppConfiguration((hostingContext, config) =>
    {
        var env = hostingContext.HostingEnvironment;
        config.SetBasePath(env.ContentRootPath)
          .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
          .AddJsonFile(path: $"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    }).UseIISIntegration().UseDefaultServiceProvider(options =>
                    options.ValidateScopes = false);
    builder.Host.UseSerilog(Log.Logger);

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddControllersWithViews(options =>
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
    builder.Services.AddDbContext<AppDbContext>().AddHealthChecks();
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddIdentity<AppUserModel, IdentityRole<string>>(options =>
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
        options.Lockout.MaxFailedAccessAttempts = config.GetSection("IdentityConfig:MaxFailedAccess").Get<int>();
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

    builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
    options.TokenLifespan = TimeSpan.FromDays(1));


    builder.Services.Configure<PasswordHasherOptions>(option =>
    {
        option.IterationCount = 11250;
    });

    var identityBuilder = builder.Services.AddIdentityServer(options =>
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

    identityBuilder.AddDeveloperSigningCredential();


    builder.Services.AddAuthentication(IdentityServerConstants.DefaultCookieAuthenticationScheme).AddGoogle(options =>
    {
        IConfigurationSection googleAuthNSection =
        config.GetSection("IdentityConfig:SocialMedia:Google");
        options.ClientId = googleAuthNSection["ClientId"];
        options.ClientSecret = googleAuthNSection["ClientSecret"];
        options.SaveTokens = true;
    }).AddFacebook(options =>
    {
        IConfigurationSection FBAuthNSection =
        config.GetSection("IdentityConfig:SocialMedia:Facebook");
        options.ClientId = FBAuthNSection["ClientId"];
        options.ClientSecret = FBAuthNSection["ClientSecret"];
        options.SaveTokens = true;
    }).AddCookie().AddLocalApi();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(IdentityServerConstants.LocalApi.PolicyName, policy =>
        {
            policy.AddAuthenticationSchemes(IdentityServerConstants.DefaultCookieAuthenticationScheme);
            policy.RequireAuthenticatedUser();
            // custom requirements
        });
    });

    builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    });

    builder.Services.Configure<IISServerOptions>(options =>
    {
        options.AutomaticAuthentication = false;
    });

    builder.Services.Configure<IISOptions>(options =>
    {
        options.ForwardClientCertificate = false;
    });


    builder.Services.AddSwaggerGen();


    var corsConfiguration = config.GetSection("CorsSiteConfiguration").GetChildren().Select(x => x.Value).ToArray();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(CorsPolicyParam.CorsPolicyName,
        builder =>
        {
            builder.WithOrigins(corsConfiguration).AllowAnyHeader().SetIsOriginAllowedToAllowWildcardSubdomains().AllowAnyMethod();
        });
    });

    builder.Services.AddHttpClient();

    builder.Services.AddHttpContextAccessor();

    builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

    builder.Services.Configure<IdentityConfig>(options => config.GetSection("IdentityConfig").Bind(options));
    builder.Services.Configure<ModuleSettings>(options => config.GetSection("ModuleConfig").Bind(options));
    builder.Services.Configure<CryptoSettings>(options => config.GetSection("CryptoConfig").Bind(options));
    var hasAWSStorage = config.GetSection("AWSConfig:Storage").Exists();
    if (hasAWSStorage)
        builder.Services.AddSingleton<IAWSFileUploader, AWSFileUploader>();
    builder.Services.AddTransient<IProfileService, IdentityProfileService>();
    builder.Services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, Swagger>();
    builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddTransient<ICryptography, Cryptography>();
    builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();
    builder.Services.AddSingleton<IEmailService, EmailService>();
    builder.Services.AddHttpClient<IConfigurationService, ConfigurationService>()
       .SetHandlerLifetime(TimeSpan.FromMinutes(5));

    var app = builder.Build();
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<AppDbContext>();
        var configDb = config.GetSection("DbConfig");
        var migration = configDb.GetValue<bool>("Migrate");
        var seed = configDb.GetValue<bool>("Seed");
        if (context.Database.IsSqlServer() && migration)
        {
            context.Database.Migrate();
        }

        if (app.Environment.IsDevelopment())
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
        if (seed)
        {
            var userManager = services.GetRequiredService<UserManager<AppUserModel>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<string>>>();

            ModelBuilderExtensionsHelper.SeedData(context, userManager, roleManager, config).Wait();
        }
    }
    var forwardOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        RequireHeaderSymmetry = false
    };

    forwardOptions.KnownNetworks.Clear();
    forwardOptions.KnownProxies.Clear();

    app.UseForwardedHeaders(forwardOptions);

    app.UseStaticFiles();
    #region swagger configuration 


    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        var swaggerEndPointv1 = $"/swagger/v1/swagger.json";
        if (app.Environment.IsStaging() || app.Environment.IsProduction())
        {
            swaggerEndPointv1 = $"/swagger/v1/swagger.json";
        }
        options.SwaggerEndpoint(swaggerEndPointv1, $"Api v1");
        options.RoutePrefix = "swagger";
        options.InjectStylesheet("/swagger/ui/custom.css");
        options.InjectJavascript("/swagger/ui/custom.js");
        options.DocumentTitle = "API endpoint Collection";
        options.OAuthUsePkce();

    });

    #endregion

    app.UseRouting();

    app.UseCors(CorsPolicyParam.CorsPolicyName);

    app.UseIdentityServer();

    app.UseCookiePolicy();

    app.UseAuthorization();

    app.UseSerilogRequestLogging();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapDefaultControllerRoute();

    });
    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
static ElasticsearchSinkOptions ConfigureElasticSink(IConfigurationRoot configuration, string environment)
{
    return new ElasticsearchSinkOptions(new Uri(configuration["ElasticConfiguration:Uri"]))
    {
        AutoRegisterTemplate = true,
        IndexFormat = $"{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{environment?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}"
    };
}