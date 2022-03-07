using EventAuthServer.Entity;
using EventAuthServer.infrastructure.library.Persistence.SeedData;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace EventAuthServer.ModelBuilderExtension
{
    public static class ModelBuilderExtensionsHelper
    {
        public static void DescriptionProperty(this ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MenuDetails());
            
        }
        public static async Task SeedData(AppDbContext context,
            UserManager<AppUserModel> userManager,
            RoleManager<IdentityRole<string>> roleManager)
        {
            try
            {
                await SeedRole.SeedData(context, roleManager);
                await SeedUser.SeedData(context, userManager);
                SeedApiResourceClient.SeedData(context);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
