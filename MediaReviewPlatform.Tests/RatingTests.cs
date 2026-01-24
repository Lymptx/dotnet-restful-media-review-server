using dotnet_restful_media_review_server.System;
using Xunit;
using FluentAssertions;

namespace MediaReviewPlatform.Tests
{
    public class MediaEntryTests
    {
        [Fact]
        public void MediaEntry_DefaultValues_ShouldBeInitialized()
        {
            var media = new MediaEntry();

            media.Title.Should().BeEmpty();
            media.MediaType.Should().BeEmpty();
            media.Genre.Should().BeEmpty();
            media.Description.Should().BeEmpty();
            media.ReleaseYear.Should().Be(0);
            media.AgeRestriction.Should().Be(0);
        }

        [Fact]
        public void MediaEntry_CreatedAt_ShouldBeNearUtcNow()
        {
            var media = new MediaEntry();

            media.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [Fact]
        public void MediaEntry_WithValidData_ShouldSetPropertiesCorrectly()
        {
            var media = new MediaEntry
            {
                Title = "Test Movie",
                MediaType = "movie",
                Genre = "Action",
                ReleaseYear = 2024,
                AgeRestriction = 12,
                Description = "Test description",
                CreatorUserId = 1
            };

            media.Title.Should().Be("Test Movie");
            media.MediaType.Should().Be("movie");
            media.Genre.Should().Be("Action");
            media.ReleaseYear.Should().Be(2024);
            media.AgeRestriction.Should().Be(12);
            media.Description.Should().Be("Test description");
            media.CreatorUserId.Should().Be(1);
        }

        [Fact]
        public void MediaEntry_UpdatedAt_ShouldBeNullByDefault()
        {
            var media = new MediaEntry();

            media.UpdatedAt.Should().BeNull();
        }

        [Fact]
        public void MediaEntry_UpdatedAt_CanBeSet()
        {
            var media = new MediaEntry
            {
                UpdatedAt = DateTime.UtcNow
            };

            media.UpdatedAt.Should().NotBeNull();
            media.UpdatedAt.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }
}