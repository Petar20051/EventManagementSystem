using EventMaganementSystem.Controllers;
using EventManagementSystem.Core.Contracts;
using EventManagementSystem.Core.Models;
using EventManagementSystem.Core.Models.Account;
using EventManagementSystem.Infrastructure.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests
{
    public class ViewTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ViewTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/")]
        [InlineData("/Home/Index")]
        [InlineData("/Events/Index")]
        [InlineData("/Home/Privacy")]
        [InlineData("/Notifications/Index")]
        [InlineData("/Events/Search")]
        [InlineData("/Payments/PaymentFailed")]
        [InlineData("/Payments/PaymentSuccess")]
        public async Task AllViews_ShouldLoadSuccessfully(string url)
        {
            
            var client = _factory.CreateClient();

           
            var response = await client.GetAsync(url);

           
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content));
        }

        [Theory]
        [InlineData("/Identity/Account/Login")]
        [InlineData("/Identity/Account/Register")]
        [InlineData("/Identity/Account/Logout")]
        public async Task IdentityViews_ShouldLoadSuccessfully(string url)
        {
            
            var client = _factory.CreateClient();

            
            var response = await client.GetAsync(url);

           
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }
}
