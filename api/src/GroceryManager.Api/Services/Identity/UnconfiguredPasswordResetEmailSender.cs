namespace GroceryManager.Api.Services.Identity;

public sealed class UnconfiguredPasswordResetEmailSender : IPasswordResetEmailSender
{
    public Task SendAsync(
        string recipientEmail,
        string resetUrl,
        CancellationToken cancellationToken) =>
        throw new InvalidOperationException("A production password-reset email provider has not been configured.");
}
