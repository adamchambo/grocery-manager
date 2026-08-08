using System.ComponentModel.DataAnnotations;

namespace GroceryManager.Api.Dtos.Identity;

public sealed record RegisterAccountRequest(
    [property: Required, EmailAddress, StringLength(256)] string Email,
    [property: Required, StringLength(100, MinimumLength = 6),
     RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
         ErrorMessage = "Password must contain uppercase, lowercase, number, and special characters.")]
    string Password);

public sealed record LoginRequest(
    [property: Required, EmailAddress, StringLength(256)] string Email,
    [property: Required, StringLength(100, MinimumLength = 6)] string Password);

public sealed record ForgotPasswordRequest(
    [property: Required, EmailAddress, StringLength(256)] string Email);

public sealed record ResetPasswordRequest(
    [property: Required, EmailAddress, StringLength(256)] string Email,
    [property: Required] string Token,
    [property: Required, StringLength(100, MinimumLength = 6),
     RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
         ErrorMessage = "Password must contain uppercase, lowercase, number, and special characters.")]
    string NewPassword);

public sealed record UpdateAccountRequest(
    [property: Required, EmailAddress, StringLength(256)] string Email);

public sealed record AccountResponse(Guid Id, string Email);
