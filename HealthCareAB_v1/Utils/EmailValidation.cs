using System.Text.RegularExpressions;

namespace HealthCareAB_v1.Utils;

public static class EmailValidator
{
    private static readonly Regex Pattern = new(
        @"^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static bool IsValid(string? email) =>
        !string.IsNullOrWhiteSpace(email) &&
        email.Length <= 254 &&
        Pattern.IsMatch(email.Trim());
}
