using EventManagementSystem.Core.Services;
using EventManagementSystem.Infrastructure.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventManagementSystem.Tests.ServicesTests
{
    public class DiscountServiceTests
    {
        private readonly DiscountService _discountService;

        public DiscountServiceTests()
        {
            _discountService = new DiscountService();
        }

        [Theory]
        [InlineData(SponsorshipTier.Bronze, 100, 95)]
        [InlineData(SponsorshipTier.Silver, 200, 180)] 
        [InlineData(SponsorshipTier.Gold, 300, 255)] 
        [InlineData(null, 400, 400)]
        public async Task ApplyDiscountAsync_ShouldApplyCorrectDiscount(SponsorshipTier? tier, decimal originalPrice, decimal expectedPrice)
        {
           
            var result = await _discountService.ApplyDiscountAsync(tier, originalPrice);

            
            Assert.Equal(expectedPrice, result);
        }

        [Fact]
        public async Task ApplyDiscountAsync_ShouldReturnOriginalPrice_WhenPriceIsZero()
        {
           
            var originalPrice = 0m;

           
            var result = await _discountService.ApplyDiscountAsync(SponsorshipTier.Gold, originalPrice);

            
            Assert.Equal(0m, result);
        }

        [Fact]
        public async Task ApplyDiscountAsync_ShouldReturnOriginalPrice_WhenTierIsNull()
        {
            
            var originalPrice = 100m;

           
            var result = await _discountService.ApplyDiscountAsync(null, originalPrice);

            
            Assert.Equal(originalPrice, result);
        }

        [Theory]
        [InlineData(SponsorshipTier.Bronze, 5)]  
        [InlineData(SponsorshipTier.Silver, 10)] 
        [InlineData(SponsorshipTier.Gold, 15)]   
        [InlineData(null, 0)]                   
        public void GetSponsorDiscountPercentage_ShouldReturnCorrectPercentage(SponsorshipTier? tier, int expectedPercentage)
        {
           
            var method = typeof(DiscountService).GetMethod("GetSponsorDiscountPercentage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = (int)method.Invoke(_discountService, new object[] { tier });

           
            Assert.Equal(expectedPercentage, result);
        }
    }
}
