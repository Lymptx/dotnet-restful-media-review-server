using dotnet_restful_media_review_server.System;
using Xunit;
using FluentAssertions;

namespace MediaReviewPlatform.Tests
{
    public class UserTests
    {
        [Fact]
        public void SetPassword_ShouldHashPassword()
        {
            var user = new User { UserName = "testuser" };
            user.SetPassword("password123");

            user.PasswordHash.Should().NotBeNullOrEmpty();
            user.PasswordSalt.Should().NotBeNullOrEmpty();
            user.PasswordHash.Should().NotBe("password123");
        }

        [Fact]
        public void SetPassword_WithEmptyPassword_ShouldThrowException()
        {
            var user = new User { UserName = "testuser" };

            Action act = () => user.SetPassword("");

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SetPassword_WithNullPassword_ShouldThrowException()
        {
            var user = new User { UserName = "testuser" };

            Action act = () => user.SetPassword(null);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
        {
            var user = new User { UserName = "testuser" };
            user.SetPassword("password123");

            bool result = user.VerifyPassword("password123");

            result.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
        {
            var user = new User { UserName = "testuser" };
            user.SetPassword("password123");

            bool result = user.VerifyPassword("wrongpassword");

            result.Should().BeFalse();
        }

        [Fact]
        public void VerifyPassword_WithEmptyPassword_ShouldReturnFalse()
        {
            var user = new User { UserName = "testuser" };
            user.SetPassword("password123");

            bool result = user.VerifyPassword("");

            result.Should().BeFalse();
        }

        [Fact]
        public void SetPassword_TwiceWithDifferentPasswords_ShouldGenerateDifferentHashes()
        {
            var user = new User { UserName = "testuser" };
            user.SetPassword("password1");
            string firstHash = user.PasswordHash;

            user.SetPassword("password2");
            string secondHash = user.PasswordHash;

            secondHash.Should().NotBe(firstHash);
        }

        [Fact]
        public void User_DefaultCreatedAt_ShouldBeNearUtcNow()
        {
            var user = new User();

            user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }
    }
}