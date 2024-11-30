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
        [InlineData(SponsorshipTier.Bronze, 100, 95)] // 5% discount
        [InlineData(SponsorshipTier.Silver, 200, 180)] // 10% discount
        [InlineData(SponsorshipTier.Gold, 300, 255)] // 15% discount
        [InlineData(null, 400, 400)] // No discount
        public async Task ApplyDiscountAsync_ShouldApplyCorrectDiscount(SponsorshipTier? tier, decimal originalPrice, decimal expectedPrice)
        {
            // Act
            var result = await _discountService.ApplyDiscountAsync(tier, originalPrice);

            // Assert
            Assert.Equal(expectedPrice, result);
        }

        [Fact]
        public async Task ApplyDiscountAsync_ShouldReturnOriginalPrice_WhenPriceIsZero()
        {
            // Arrange
            var originalPrice = 0m;

            // Act
            var result = await _discountService.ApplyDiscountAsync(SponsorshipTier.Gold, originalPrice);

            // Assert
            Assert.Equal(0m, result);
        }

        [Fact]
        public async Task ApplyDiscountAsync_ShouldReturnOriginalPrice_WhenTierIsNull()
        {
            // Arrange
            var originalPrice = 100m;

            // Act
            var result = await _discountService.ApplyDiscountAsync(null, originalPrice);

            // Assert
            Assert.Equal(originalPrice, result);
        }

        [Theory]
        [InlineData(SponsorshipTier.Bronze, 5)]  // 5% discount
        [InlineData(SponsorshipTier.Silver, 10)] // 10% discount
        [InlineData(SponsorshipTier.Gold, 15)]   // 15% discount
        [InlineData(null, 0)]                    // No discount
        public void GetSponsorDiscountPercentage_ShouldReturnCorrectPercentage(SponsorshipTier? tier, int expectedPercentage)
        {
            // Act (using reflection to test the private method)
            var method = typeof(DiscountService).GetMethod("GetSponsorDiscountPercentage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var result = (int)method.Invoke(_discountService, new object[] { tier });

            // Assert
            Assert.Equal(expectedPercentage, result);
        }
    }
}
