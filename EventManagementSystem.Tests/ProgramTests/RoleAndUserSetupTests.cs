using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ProgramTests
{
    public class RoleAndUserSetupTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public RoleAndUserSetupTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task EnsureRolesAndAdminUser_CreatesAdminRoleAndUser()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Act
            var adminRoleExists = await roleManager.RoleExistsAsync("Admin");
            var adminUser = await userManager.FindByEmailAsync("admin@example.com");

            // Assert
            Assert.True(adminRoleExists, "Admin role should exist.");
            Assert.NotNull(adminUser);
            Assert.True(await userManager.IsInRoleAsync(adminUser, "Admin"), "Admin user should be in Admin role.");
        }
    }
}