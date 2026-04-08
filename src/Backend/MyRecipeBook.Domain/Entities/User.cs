namespace MyRecipeBook.Domain.Entities;
public class User : EntityBase
{
    private string _email = string.Empty;

    public string Name { get; private set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public MyRecipeBook.Domain.ValueObjects.Email Email => new(_email);
    public string Password { get; private set; } = string.Empty;
    public Guid UserIdentifier { get; private set; } = Guid.NewGuid();

    public static User Create(string name, MyRecipeBook.Domain.ValueObjects.Email email, string encryptedPassword)
    {
        var user = new User();
        user.UpdateProfile(name, email);
        user.ChangePassword(encryptedPassword);

        return user;
    }

    public void UpdateProfile(string name, MyRecipeBook.Domain.ValueObjects.Email email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("User name cannot be empty.", nameof(name));

        Name = name.Trim();
        _email = email.Value;
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
