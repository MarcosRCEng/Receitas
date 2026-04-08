using System.Net.Mail;

namespace MyRecipeBook.Domain.ValueObjects;

public readonly record struct Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty.", nameof(value));

        var normalizedValue = value.Trim();

        try
        {
            var mailAddress = new MailAddress(normalizedValue);

            if (mailAddress.Address != normalizedValue)
                throw new ArgumentException("Email is invalid.", nameof(value));
        }
        catch (FormatException)
        {
            throw new ArgumentException("Email is invalid.", nameof(value));
        }

        Value = normalizedValue;
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
