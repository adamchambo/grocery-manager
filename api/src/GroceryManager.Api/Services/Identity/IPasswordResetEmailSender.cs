namespace GroceryManager.Api.Services.Identity;

public interface IPasswordResetEmailSender
{
    public Task SendAsync(
        string recipientEmail,
        string resetUrl,
        CancellationToken cancellationToken);
}
