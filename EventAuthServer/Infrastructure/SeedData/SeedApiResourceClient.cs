using EventAuthServer;
using EventAuthServer.Datum.Static;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using System;
using System.Linq;
using ApiResource = IdentityServer4.EntityFramework.Entities.ApiResource;
using Client = IdentityServer4.EntityFramework.Entities.Client;
using IdentityResource = IdentityServer4.EntityFramework.Entities.IdentityResource;

namespace EventAuthServer.infrastructure.library.Persistence.SeedData
{
    public static class SeedApiResourceClient
    {
        public static void SeedData(AppDbContext context)
        {
            try
            {
                context.Database.EnsureCreated();
                var clients = ApiResourceClient.GetClients().ToList();
                clients.RemoveAll(x => context.Clients.Select(x => x.ClientId).Contains(x.ClientId));
                var identityResources = ApiResourceClient.GetIdentityResources().ToList();
                identityResources.RemoveAll(x => context.IdentityResources.Select(x => x.Name).Contains(x.Name));
                var apiResources = ApiResourceClient.GetApiResources().ToList();
                apiResources.RemoveAll(x => context.ApiResources.Select(x => x.Name).Contains(x.Name));
                if (clients.Count > 0)
                {
                    foreach (var client in clients)
                    {
                        Client entityClient = new()
                        {
                            ClientId = client.ClientId,
                            ClientName = client.ClientName,
                            Description = client.Description,
                            Enabled = client.Enabled,
                            ProtocolType = client.ProtocolType,
                            RequireClientSecret = client.RequireClientSecret,
                            ClientSecrets = client.ClientSecrets?.Select(secret => new ClientSecret
                            {
                                Value = secret.Value,
                                Type = secret.Type,
                                Description = secret.Description,
                                Expiration = secret.Expiration
                            }).ToList(),
                            RedirectUris = client.RedirectUris?.Select(uri => new ClientRedirectUri { RedirectUri = uri }).ToList(),
                            PostLogoutRedirectUris = client.PostLogoutRedirectUris?.Select(uri => new ClientPostLogoutRedirectUri { PostLogoutRedirectUri = uri }).ToList(),
                            AllowedGrantTypes = client.AllowedGrantTypes?.Select(grant => new ClientGrantType { GrantType = grant }).ToList(),
                            AllowedScopes = client.AllowedScopes?.Select(scope => new ClientScope { Scope = scope }).ToList(),
                            RequireConsent = client.RequireConsent,
                            AllowOfflineAccess = client.AllowOfflineAccess,
                            AccessTokenLifetime = client.AccessTokenLifetime,
                            IdentityTokenLifetime = client.IdentityTokenLifetime,
                            AllowedCorsOrigins = client.AllowedCorsOrigins?.Select(origin => new ClientCorsOrigin { Origin = origin }).ToList(),
                            RequirePkce = client.RequirePkce,
                            AllowPlainTextPkce = client.AllowPlainTextPkce,
                            AccessTokenType = (int)client.AccessTokenType,
                            AlwaysIncludeUserClaimsInIdToken = client.AlwaysIncludeUserClaimsInIdToken

                        };
                        context.Clients.AddRange(entityClient);
                    }
                    context.SaveChanges();
                }

                if (identityResources.Count > 0)
                {
                    foreach (var resource in identityResources)
                    {
                        IdentityResource identityResourceEntity = new IdentityResource
                        {
                            Name = resource.Name,
                            DisplayName = resource.DisplayName,
                            Description = resource.Description,
                            Enabled = resource.Enabled,
                            Required = resource.Required,
                            Emphasize = resource.Emphasize,
                            ShowInDiscoveryDocument = resource.ShowInDiscoveryDocument,
                            Created = DateTime.UtcNow,
                            Updated = DateTime.UtcNow,
                            UserClaims = resource.UserClaims.Select(claim => new IdentityResourceClaim
                            {
                                Type = claim
                            }).ToList()
                        };
                        context.IdentityResources.Add(identityResourceEntity);
                    }
                    context.SaveChanges();
                }

                if (apiResources.Count > 0)
                {
                    foreach (var apiResource in apiResources)
                    {
                        ApiResource identityApiResourceEntity = new ApiResource
                        {
                            Name = apiResource.Name,
                            DisplayName = apiResource.DisplayName,
                            Description = apiResource.Description,
                            Enabled = apiResource.Enabled,
                            Scopes = apiResource.Scopes.Any()? apiResource.Scopes.Select(s => new ApiResourceScope
                            {
                                Scope = s
                            }).ToList(): null,
                            Secrets = apiResource.ApiSecrets.Any() ? apiResource.ApiSecrets.Select(s => new ApiResourceSecret
                            {
                                Value = s.Value,
                                Type = s.Type,
                                Description = s.Description,
                                Expiration = s.Expiration
                            }).ToList(): null,
                            UserClaims = apiResource.UserClaims.Any() ? apiResource.UserClaims.Select(c => new ApiResourceClaim
                            {
                                Type = c
                            }).ToList(): null,
                            Properties = apiResource.Properties.Any() ? apiResource.Properties.Select(p => new ApiResourceProperty
                            {
                                Key = p.Key,
                                Value = p.Value
                            }).ToList(): null
                        };
                        context.ApiResources.Add(identityApiResourceEntity);
                    }
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
    }
}
