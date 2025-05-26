using dosyayonetim.api.Models;
using dosyayonetim.api.Models.Enums;
using Microsoft.AspNetCore.Identity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace dosyayonetim.api.Data
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider service)
        {
            //Seed Roles
            var userManager = service.GetService<UserManager<ApplicationUser>>();
            var roleManager = service.GetService<RoleManager<IdentityRole>>();

            await roleManager.CreateAsync(new IdentityRole(Roles.Admin));
            await roleManager.CreateAsync(new IdentityRole(Roles.User));

            
            // Seed Default Admin
            var admin = new ApplicationUser
            {
                UserName = "admin@example.com",
                Email = "admin@example.com",
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(admin.Email) == null)
            {
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, Roles.Admin);
            }

            var anonymousUser = new ApplicationUser
            {
                UserName = "anonymous",
                Email = "anonymous@system.com",
                FirstName = "Anonim",
                LastName = "Kullan�c�",
                EmailConfirmed = true,
                Id = "anonymous" // �zel ID at�yoruz
            };

            if (await userManager.FindByIdAsync("anonymous") == null)
            {
                await userManager.CreateAsync(anonymousUser, "Anonymous@123");
                await userManager.AddToRoleAsync(anonymousUser, Roles.User);
            }

                // Seed Default User
             var user = new ApplicationUser
            {
                UserName = "user@example.com",
                Email = "user@example.com",
                FirstName = "System",
                LastName = "User",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(user.Email) == null)
            {
                await userManager.CreateAsync(user, "User@123");
                await userManager.AddToRoleAsync(user, Roles.User);
            }
        }
    }
} 