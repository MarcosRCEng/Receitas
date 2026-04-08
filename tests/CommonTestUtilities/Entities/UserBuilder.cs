using Bogus;
using CommonTestUtilities.Cryptography;
using MyRecipeBook.Domain.Entities;
using MyRecipeBook.Domain.ValueObjects;

namespace CommonTestUtilities.Entities;

public class UserBuilder
{
    public static (User user, string password) Build()
    {
        var passwordEncripter = PasswordEncripterBuilder.Build();
        var faker = new Faker();
        var password = faker.Internet.Password();
        var name = faker.Person.FirstName;
        var email = faker.Internet.Email(name);

        var user = User.Create(name, new Email(email), passwordEncripter.Encrypt(password));
        user.Id = 1;

        return (user, password);
    }
}
