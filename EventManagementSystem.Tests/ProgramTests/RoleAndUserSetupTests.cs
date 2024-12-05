using EventMaganementSystem.Data;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ProgramTests
{
    public class RoleAndUserSetupTests
    {
        [Fact]
        public async Task RolesAndAdminUser_AreSetUpCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();

            // Add in-memory database for EventDbContext
            services.AddDbContext<EventDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));

            // Add Identity services with in-memory stores
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<EventDbContext>()
                .AddDefaultTokenProviders();

            // Add logging services
            services.AddLogging();

            var serviceProvider = services.BuildServiceProvider();

            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Act
            await Program.EnsureRolesAndAdminUser(roleManager, userManager);

            // Assert
            var adminRoleExists = await roleManager.RoleExistsAsync("Admin");
            Assert.True(adminRoleExists);

            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            Assert.NotNull(adminUser);
            Assert.True(await userManager.IsInRoleAsync(adminUser, "Admin"));
        }
    }

}