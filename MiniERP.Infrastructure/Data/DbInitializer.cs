using Microsoft.EntityFrameworkCore;
using MiniERP.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MiniERP.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // Run migrations automatically
            await context.Database.MigrateAsync();

            // 1. Seed Roles
            var roleNames = new[] { "ADMIN", "STAFF", "ACCOUNTANT", "MANAGER" };
            foreach (var roleName in roleNames)
            {
                var roleExists = await context.Roles.AnyAsync(r => r.RoleName == roleName);
                if (!roleExists)
                {
                    context.Roles.Add(new Role { RoleName = roleName });
                }
            }
            await context.SaveChangesAsync();

            // 2. Ensure admin user exists and has the ADMIN role
            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "ADMIN");
            if (adminRole != null)
            {
                var adminUser = await context.Users
                    .Include(u => u.UserRoles)
                    .FirstOrDefaultAsync(u => u.Username == "admin1" || u.Username == "admin");

                if (adminUser != null)
                {
                    var hasAdminRole = adminUser.UserRoles.Any(ur => ur.RoleId == adminRole.Id);
                    if (!hasAdminRole)
                    {
                        context.UserRoles.Add(new UserRole
                        {
                            UserId = adminUser.Id,
                            RoleId = adminRole.Id
                        });
                        await context.SaveChangesAsync();
                    }
                }
                else
                {
                    // Create default admin user if none exists
                    var passwordHash = BCrypt.Net.BCrypt.HashPassword("admin123");
                    var newAdmin = new User
                    {
                        Username = "admin",
                        Email = "admin@example.com",
                        FullName = "Administrator",
                        PasswordHash = passwordHash,
                        IsActive = true
                    };
                    context.Users.Add(newAdmin);
                    await context.SaveChangesAsync();

                    context.UserRoles.Add(new UserRole
                    {
                        UserId = newAdmin.Id,
                        RoleId = adminRole.Id
                    });
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
