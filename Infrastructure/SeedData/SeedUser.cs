using EventAuthServer;
using EventAuthServer.Datum.Enum;
using EventAuthServer.Entity;
using med.common.library.constant;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace EventAuthServer.infrastructure.library.Persistence.SeedData
{
    public static class SeedUser
    {
        public static async Task SeedData(AppDbContext context, UserManager<AppUserModel> userManager)
        {
            context.Database.EnsureCreated();

            const string ADMIN_ID = "b5bdf785-07e8-4840-9b53-b8af8a858f94";
            var userManagerData = await userManager.FindByIdAsync(ADMIN_ID);
            if (userManagerData == null)
            {
                var userdata = new AppUserModel
                {
                    Id = ADMIN_ID,
                    AccessFailedCount = 0,
                    LockoutEnabled = false,
                    FullName = "Admin",
                    NickName = "Admin",
                    UserName = "Admin",
                    Email = "saugatdahal20@gmail.com",
                    EmailConfirmed = true,
                    Status = (int)AccountStatusEnum.Verified,
                    PhoneNumberConfirmed = true,
                    NormalizedEmail = "SAUGATDAHAL20@GMAIL.COM",
                    NormalizedUserName = "ADMIN",
                    SecurityStamp = "ce907fd5-ccb4-4e96-a7ea-45712a14f5ef",
                    ConcurrencyStamp = "32fe9448-0c6c-43b2-b605-802c19c333a6",
                    CreatedDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(userdata, "Admin@123");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(userdata, IdentityRoleConstant.SuperAdmin);
                }
            }
        }
    }
}
