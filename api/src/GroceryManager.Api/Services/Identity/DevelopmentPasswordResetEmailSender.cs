namespace GroceryManager.Api.Services.Identity;

public sealed class DevelopmentPasswordResetEmailSender(
    ILogger<DevelopmentPasswordResetEmailSender> logger) : IPasswordResetEmailSender
{
    private static readonly Action<ILogger, string, string, Exception?> LogPasswordResetUrl =
        LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            new EventId(1001, nameof(LogPasswordResetUrl)),
            "Development password reset for {RecipientEmail}: {ResetUrl}");

    public Task SendAsync(
        string recipientEmail,
        string resetUrl,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LogPasswordResetUrl(logger, recipientEmail, resetUrl, null);
        return Task.CompletedTask;
    }
}
