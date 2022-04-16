using EventAuthServer;
using EventAuthServer.Datum.Static;
using IdentityServer4.EntityFramework.Mappers;
using System;
using System.Linq;

namespace EventAuthServer.infrastructure.library.Persistence.SeedData
{
    public static class SeedApiResourceClient
    {
        public static void SeedData(AppDbContext context)
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
                    context.Clients.AddRange(client.ToEntity());
                }
                context.SaveChanges();
            }

            if (identityResources.Count > 0)
            {
                foreach (var resource in identityResources)
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (apiResources.Count > 0)
            {
                foreach (var resource in apiResources)
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

        }
    }
}
