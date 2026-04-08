namespace MyRecipeBook.Domain.Entities;
public class User : EntityBase
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Password { get; private set; } = string.Empty;
    public Guid UserIdentifier { get; private set; } = Guid.NewGuid();

    public static User Create(string name, string email, string encryptedPassword)
    {
        var user = new User();
        user.UpdateProfile(name, email);
        user.ChangePassword(encryptedPassword);

        return user;
    }

    public void UpdateProfile(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("User name cannot be empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("User email cannot be empty.", nameof(email));

        Name = name.Trim();
        Email = email.Trim();
    }

    public void ChangePassword(string encryptedPassword)
    {
        if (string.IsNullOrWhiteSpace(encryptedPassword))
            throw new ArgumentException("User password cannot be empty.", nameof(encryptedPassword));

        Password = encryptedPassword;
    }

    public void Deactivate()
    {
        Active = false;
    }
}
