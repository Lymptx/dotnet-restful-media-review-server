using dotnet_restful_media_review_server.System;
using Xunit;
using FluentAssertions;

namespace MediaReviewPlatform.Tests
{
    public class RatingTests
    {
        [Fact]
        public void Rating_DefaultValues_ShouldBeInitialized()
        {
            var rating = new Rating();

            rating.Stars.Should().Be(0);
            rating.Comment.Should().BeEmpty();
            rating.IsConfirmed.Should().BeFalse();
            rating.LikeCount.Should().Be(0);
        }

        [Fact]
        public void Rating_CreatedAt_ShouldBeNearUtcNow()
        {
            var rating = new Rating();

            rating.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Rating_WithValidData_ShouldSetPropertiesCorrectly()
        {
            var rating = new Rating
            {
                MediaId = 1,
                UserId = 1,
                Stars = 5,
                Comment = "Great movie!",
                IsConfirmed = true,
                LikeCount = 10
            };

            rating.MediaId.Should().Be(1);
            rating.UserId.Should().Be(1);
            rating.Stars.Should().Be(5);
            rating.Comment.Should().Be("Great movie!");
            rating.IsConfirmed.Should().BeTrue();
            rating.LikeCount.Should().Be(10);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        public void Rating_StarsValue_ShouldAcceptValidRange(int stars)
        {
            var rating = new Rating { Stars = stars };

            rating.Stars.Should().Be(stars);
            rating.Stars.Should().BeInRange(1, 5);
        }

        [Fact]
        public void Rating_UpdatedAt_ShouldBeNullByDefault()
        {
            var rating = new Rating();

            rating.UpdatedAt.Should().BeNull();
        }

        [Fact]
        public void Rating_UpdatedAt_CanBeSet()
        {
            var rating = new Rating
            {
                UpdatedAt = DateTime.UtcNow
            };

            rating.UpdatedAt.Should().NotBeNull();
            rating.UpdatedAt.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void Rating_Comment_CanBeEmpty()
        {
            var rating = new Rating
            {
                Stars = 4,
                Comment = ""
            };

            rating.Comment.Should().BeEmpty();
        }

        [Fact]
        public void Rating_IsConfirmed_ShouldDefaultToFalse()
        {
            var rating = new Rating();

            rating.IsConfirmed.Should().BeFalse();
        }
    }
}