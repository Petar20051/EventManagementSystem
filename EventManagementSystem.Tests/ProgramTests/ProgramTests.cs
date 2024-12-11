using EventMaganementSystem.Data;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ProgramTests
{
    public class ProgramTests
    {
        [Fact]
        public void Program_Main_SetsUpBuilderCorrectly()
        {
            
            var builder = WebApplication.CreateBuilder();

            
            builder.Services.AddDbContext<EventDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));

            
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object,
                null, null, null, null, null, null, null, null);
            builder.Services.AddSingleton(userManagerMock.Object);

            
            var roleStoreMock = new Mock<IRoleStore<IdentityRole>>();
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                roleStoreMock.Object,
                null, null, null, null);
            builder.Services.AddSingleton(roleManagerMock.Object);

            
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication("TestScheme")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            builder.Services.AddSignalR(); 
            builder.Services.AddLogging(); 

            
            Program.RegisterCustomServices(builder.Services);

            
            var serviceProvider = builder.Services.BuildServiceProvider();
            Assert.NotNull(serviceProvider.GetService<IEventService>()); 
            Assert.NotNull(serviceProvider.GetService<UserManager<ApplicationUser>>()); 
        }
    }

  
   

}
