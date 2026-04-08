using CommonTestUtilities.Entities;
using FluentAssertions;
using Xunit;

namespace UseCases.Test.User.Entity;

public class UserEntityTest
{
    [Fact]
    public void ChangePassword_Should_Update_StoredPassword()
    {
        (var user, _) = UserBuilder.Build();

        user.ChangePassword("encrypted-password");

        user.Password.Should().Be("encrypted-password");
    }

    [Fact]
    public void Deactivate_Should_Set_Active_False()
    {
        (var user, _) = UserBuilder.Build();

        user.Deactivate();

        user.Active.Should().BeFalse();
    }
}
