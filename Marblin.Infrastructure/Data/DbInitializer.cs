using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Marblin.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure the database is created (optional, depending on migration strategy)
            // var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            // await context.Database.MigrateAsync();

            // Ensure Admin role exists
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            // Check if any users exist
            if (await userManager.Users.AnyAsync())
            {
                // Optional: Ensure the existing admin has the role if it's the specific email
                var existingAdmin = await userManager.FindByEmailAsync("admin@marblin.com");
                if (existingAdmin != null && !await userManager.IsInRoleAsync(existingAdmin, "Admin"))
                {
                    await userManager.AddToRoleAsync(existingAdmin, "Admin");
                }
                return; 
            }

            // Create default Admin user
            var adminUser = new IdentityUser
            {
                UserName = "admin@marblin.com",
                Email = "admin@marblin.com",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Marblin@2026"); // Default secure password

            if (result.Succeeded)
            {
                 await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create default admin user: {errors}");
            }
            
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            await SeedSiteSettingsAsync(context);
        }

        private static async Task SeedSiteSettingsAsync(ApplicationDbContext context)
        {
            if (!await context.SiteSettings.AnyAsync())
            {
                var settings = new Marblin.Core.Entities.SiteSettings
                {
                    DepositPercentage = 25,
                    HeroHeadline = "Timeless Elegance",
                    HeroSubheadline = "Handcrafted marble and stone artifacts.",
                    InstapayAccount = "N/A"
                };

                context.SiteSettings.Add(settings);
                await context.SaveChangesAsync();
            }
        }
    }
}
