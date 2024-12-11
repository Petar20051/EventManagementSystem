using EventMaganementSystem;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ProgramTests
{
    public class SignalRTests 
    {
        [Fact]
        public void SignalR_IsConfiguredCorrectly()
        {
            
            var builder = WebApplication.CreateBuilder();

            
            builder.Services.AddAuthorization(); 
            builder.Services.AddAuthentication("TestScheme") 
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });
            builder.Services.AddSignalR(); 
            builder.Services.AddLogging(); 
            builder.Services.AddRazorPages(); 
            builder.Services.AddControllersWithViews(); 

            var app = builder.Build();

            
            Program.ConfigureMiddleware(app);

            
            Assert.NotNull(app); 
        }
    }
}

