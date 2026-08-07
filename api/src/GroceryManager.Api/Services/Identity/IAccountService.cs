using GroceryManager.Api.Dtos.Identity;

namespace GroceryManager.Api.Services.Identity;

public interface IAccountService
{
    public Task<AccountResponse> RegisterAsync(RegisterAccountRequest request, CancellationToken cancellationToken);
    public Task<AccountResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    public Task LogoutAsync(CancellationToken cancellationToken);
    public Task SendPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken);
    public Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken);
    public Task<AccountResponse> GetCurrentAsync(CancellationToken cancellationToken);
    public Task<AccountResponse> UpdateCurrentAsync(UpdateAccountRequest request, CancellationToken cancellationToken);
}
