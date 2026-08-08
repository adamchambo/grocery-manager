namespace GroceryManager.Api.Services.Identity;

public sealed class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";

    public string ClientUrl { get; set; } = string.Empty;
}
