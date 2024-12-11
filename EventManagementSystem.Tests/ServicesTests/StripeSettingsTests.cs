using EventManagementSystem.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class StripeSettingsTests
    {
        [Fact]
        public void StripeSettings_ShouldBindCorrectlyFromConfiguration()
        {
            
            var inMemorySettings = new Dictionary<string, string>
        {
            { "Stripe:PublishableKey", "pk_test_123" },
            { "Stripe:SecretKey", "sk_test_123" }
        };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            
            var stripeSettings = new StripeSettings();
            configuration.GetSection("Stripe").Bind(stripeSettings);

            
            Assert.Equal("pk_test_123", stripeSettings.PublishableKey);
            Assert.Equal("sk_test_123", stripeSettings.SecretKey);
        }

        [Fact]
        public void StripeSettings_ShouldBeConfiguredCorrectlyThroughOptions()
        {
            
            var inMemorySettings = new Dictionary<string, string>
        {
            { "Stripe:PublishableKey", "pk_test_123" },
            { "Stripe:SecretKey", "sk_test_123" }
        };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            var options = Options.Create(configuration.GetSection("Stripe").Get<StripeSettings>());

            
            var stripeSettings = options.Value;

            
            Assert.Equal("pk_test_123", stripeSettings.PublishableKey);
            Assert.Equal("sk_test_123", stripeSettings.SecretKey);
        }
    }
}
