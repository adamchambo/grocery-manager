using GroceryManager.Api.Dtos.Identity;

namespace GroceryManager.Api.Services.Identity;

public sealed class AccountService : IAccountService
{
    public Task<AccountResponse> RegisterAsync(RegisterAccountRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<AccountResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task LogoutAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task SendPasswordResetAsync(ForgotPasswordRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<AccountResponse> GetCurrentAsync(CancellationToken cancellationToken) => throw new NotImplementedException();
    public Task<AccountResponse> UpdateCurrentAsync(UpdateAccountRequest request, CancellationToken cancellationToken) => throw new NotImplementedException();
}
