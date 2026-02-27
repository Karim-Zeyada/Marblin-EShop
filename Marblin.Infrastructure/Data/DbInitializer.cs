using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Marblin.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Only auto-migrate in development. Run migrations manually in production.
            var env = serviceProvider.GetRequiredService<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();
            if (env.EnvironmentName == "Development")
            {
                await context.Database.MigrateAsync();
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

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

            var adminPassword = configuration["SeedSettings:DefaultAdminPassword"]
                ?? throw new InvalidOperationException(
                    "SeedSettings:DefaultAdminPassword must be configured to seed the admin user.");

            var result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                 await userManager.AddToRoleAsync(adminUser, "Admin");
                 var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
                 logger.LogWarning("Default Admin user created. Change the password immediately in production.");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create default admin user: {errors}");
            }
            
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
