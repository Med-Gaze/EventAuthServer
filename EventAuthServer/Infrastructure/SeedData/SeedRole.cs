using EventAuthServer;
using med.common.library.constant;
using med.common.library.Enum;
using med.common.library.model;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventAuthServer.infrastructure.library.Persistence.SeedData
{
    public static class SeedRole
    {
        public static async Task SeedData(AppDbContext context, RoleManager<IdentityRole<string>> roleManager)
        {
            context.Database.EnsureCreated();
            var data = IdentityRoleConstant.RoleList().ToList();
            data.Add(new KeyValuePair<string, RoleViewModel>("Staff", new RoleViewModel {
                Id = "8DD28606-8EC3-4F6A-9021-2D29BB8F3ABE",
                Rank = 3
            }));
            data.RemoveAll(x => context.Roles.Select(x => x.Id).Contains(x.Key));
            if (data.Count > 0)
            {
                foreach (var item in data)
                {
                    var rolemanagerData = await roleManager.FindByNameAsync(item.Key);
                    if (rolemanagerData == null)
                    {
                        await roleManager.CreateAsync(new IdentityRole<string>
                        {
                            Id = item.Value.Id,
                            Name = item.Key,
                            NormalizedName = item.Key?.ToUpper(),
                        });
                    }
                }
            }

        }

    }
}
