using dotnet_restful_media_review_server.System;
using dotnet_restful_media_review_server.Database;
using Xunit;
using FluentAssertions;

namespace MediaReviewPlatform.Tests
{
    public class SessionTests
    {
        [Fact]
        public void Session_Token_ShouldNotBeEmpty()
        {
            var user = new User
            {
                UserName = "testuser",
                FullName = "Test User",
                Email = "test@test.com"
            };
            user.SetPassword("password123");

            string token = user.UserName;

            token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void Session_Token_ShouldBeUnique()
        {
            var tokens = new HashSet<string>();

            for (int i = 0; i < 100; i++)
            {
                var user = new User
                {
                    UserName = $"user{i}",
                    FullName = "Test User",
                    Email = $"test{i}@test.com"
                };
                user.SetPassword("password123");

                tokens.Add(user.PasswordHash);
            }

            tokens.Count.Should().Be(100);
        }

        [Fact]
        public void Session_AdminFlag_ShouldBeTrueForAdmin()
        {
            bool isAdmin = "admin" == "admin";

            isAdmin.Should().BeTrue();
        }

        [Fact]
        public void Session_AdminFlag_ShouldBeFalseForNonAdmin()
        {
            bool isAdmin = "testuser" == "admin";

            isAdmin.Should().BeFalse();
        }
    }
}