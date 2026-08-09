using GroceryManager.Api.Dtos.Identity;
using GroceryManager.Api.Entities.Identity;
using GroceryManager.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace GroceryManager.Api.Services.Identity;

public sealed class AccountService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ICurrentUserContext currentUser,
    IPasswordResetEmailSender passwordResetEmailSender,
    IOptions<PasswordResetOptions> passwordResetOptions) : IAccountService
{
    public async Task<AccountResponse> RegisterAsync(RegisterAccountRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = request.Email.Trim(), Email = request.Email.Trim() };
        ThrowIfFailed(await userManager.CreateAsync(user, request.Password));
        await signInManager.SignInAsync(user, isPersistent: false);
        return ToResponse(user);
    }

    public async Task<AccountResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByEmailAsync(request.Email.Trim())
            ?? throw new UnauthorizedAccessException("Invalid email or password.");
        var result = await signInManager.PasswordSignInAsync(user, request.Password, false, lockoutOnFailure: true);
        if (!result.Succeeded) throw new UnauthorizedAccessException("Invalid email or password.");
        return ToResponse(user);
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await signInManager.SignOutAsync();
    }

    public async Task SendPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null) return;

        if (!Uri.TryCreate(passwordResetOptions.Value.ClientUrl, UriKind.Absolute, out var clientUrl))
            throw new InvalidOperationException("PasswordReset:ClientUrl is not configured with an absolute URL.");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrl = QueryHelpers.AddQueryString(clientUrl.ToString(), new Dictionary<string, string?>
        {
            ["email"] = user.Email,
            ["token"] = token
        });
        await passwordResetEmailSender.SendAsync(user.Email!, resetUrl, cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByEmailAsync(request.Email.Trim())
            ?? throw new ArgumentException("The password reset request is invalid.");
        ThrowIfFailed(await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword));
    }

    public async Task<AccountResponse> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var user = await FindCurrentAsync();
        return ToResponse(user);
    }

    public async Task<AccountResponse> UpdateCurrentAsync(UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await FindCurrentAsync();
        user.Email = request.Email.Trim();
        user.UserName = request.Email.Trim();
        ThrowIfFailed(await userManager.UpdateAsync(user));
        return ToResponse(user);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await FindCurrentAsync();
        ThrowIfFailed(await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword));
    }

    private async Task<ApplicationUser> FindCurrentAsync()
    {
        var userId = ServiceSupport.RequireUserId(currentUser);
        return await userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedAccessException("The authenticated user no longer exists.");
    }

    private static AccountResponse ToResponse(ApplicationUser user) =>
        new(user.Id, user.Email ?? throw new InvalidOperationException("The user has no email address."));

    private static void ThrowIfFailed(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new ArgumentException(string.Join(" ", result.Errors.Select(x => x.Description)));
    }
}
