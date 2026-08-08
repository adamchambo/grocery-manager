using System.ComponentModel.DataAnnotations;

namespace GroceryManager.Api.Dtos.Identity;

public sealed record RegisterAccountRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(100, MinimumLength = 6),
     RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
         ErrorMessage = "Password must contain uppercase, lowercase, number, and special characters.")]
    string Password);

public sealed record LoginRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required, StringLength(100, MinimumLength = 6)] string Password);

public sealed record ForgotPasswordRequest(
    [Required, EmailAddress, StringLength(256)] string Email);

public sealed record ResetPasswordRequest(
    [Required, EmailAddress, StringLength(256)] string Email,
    [Required] string Token,
    [Required, StringLength(100, MinimumLength = 6),
     RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).+$",
         ErrorMessage = "Password must contain uppercase, lowercase, number, and special characters.")]
    string NewPassword);

public sealed record UpdateAccountRequest(
    [Required, EmailAddress, StringLength(256)] string Email);

public sealed record AccountResponse(Guid Id, string Email);
