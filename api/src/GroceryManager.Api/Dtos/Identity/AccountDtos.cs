namespace GroceryManager.Api.Dtos.Identity;

public sealed record RegisterAccountRequest(string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);

public sealed record UpdateAccountRequest(string Email);

public sealed record AccountResponse(Guid Id, string Email);
