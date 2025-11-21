using System.Text.RegularExpressions;

namespace BTGPactual.Shared.Extensions;

public static partial class StringExtensions
{
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    public static bool IsValidObjectId(this string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        return id.Length == 24 && Regex.IsMatch(id, "^[0-9a-fA-F]{24}$");
    }

    public static bool IsValidPhoneNumber(this string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        return Regex.IsMatch(phoneNumber, @"^\+?[1-9]\d{1,14}$");
    }
}