using med.common.library.filter;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace med.common.library.configuration.service
{
    public class Swagger : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly ModuleSettings settings;
        private readonly IdentityConfig identity;

        public Swagger(IOptions<ModuleSettings> settings, IOptions<IdentityConfig> identity)
        {
            this.settings = settings.Value;
            this.identity = identity.Value;

        }
        public void Configure(SwaggerGenOptions options)
        {
            var ModuleName = this.settings.Title;
            var Descriptions = this.settings.Description;
            var CompanyName = this.settings.CompanyName;
            var Email = this.settings.Email;
            var WebsiteUrl = this.settings.BaseUrl;
          
                options.SwaggerDoc(
                  "v1",
                    new OpenApiInfo()
                    {
                        Title = $"Med {ModuleName} API 1.0",
                        Description = Descriptions,
                        Version = "1.0",
                        Contact = new OpenApiContact
                        {
                            Name = CompanyName,
                            Email = Email,
                            Url = new Uri(WebsiteUrl),
                        }
                    });
           

            options.MapType<TimeSpan?>(() => new OpenApiSchema { Type = "string", Example = new OpenApiString("00:00:00") });


            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    Implicit = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{this.identity.Issuer}/connect/authorize"),
                        TokenUrl = new Uri($"{this.identity.Issuer}/connect/token"),
                        Scopes = new Dictionary<string, string>
                            {
                                {"swagger.access", "swagger.access"}
                            },
                    }
                }
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                         new OpenApiSecurityScheme
                         {
                             Reference = new OpenApiReference
                               {
                                 Type = ReferenceType.SecurityScheme,
                                 Id = "Bearer"
                               },
                             Scheme = "oauth2",
                             Name = "Bearer",
                             In = ParameterLocation.Header
                         },
                        new List<string>()
                    }
                });
            var assembly = Assembly.GetEntryAssembly().GetName().Name;
            // Set the comments path for the Swagger JSON and UI.
            options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            var xmlFile = $"{assembly}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
            options.DescribeAllParametersInCamelCase();
            options.OperationFilter<AuthorizationCheckOperationFilter>();
            options.OperationFilter<RemoveVersionFromParameter>();
        }
    }
    public class RemoveVersionFromParameter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (operation.Parameters.Count != 0)
            {
                var versionParameter = operation.Parameters.FirstOrDefault(p => p.Name == "version");
                if (versionParameter != null)
                {
                    operation.Parameters.Remove(versionParameter);
                }
            }
        }
    }
}
