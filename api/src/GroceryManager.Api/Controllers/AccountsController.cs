using GroceryManager.Api.Dtos.Identity;
using GroceryManager.Api.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/accounts")]
public sealed class AccountsController(IAccountService accountService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AccountResponse>> Register(
        RegisterAccountRequest request,
        CancellationToken cancellationToken) =>
        Ok(await accountService.RegisterAsync(request, cancellationToken));

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AccountResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken) =>
        Ok(await accountService.LoginAsync(request, cancellationToken));

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await accountService.LogoutAsync(cancellationToken);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await accountService.SendPasswordResetAsync(request, cancellationToken);
        return NoContent();
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await accountService.ResetPasswordAsync(request, cancellationToken);
        return NoContent();
    }

    [HttpGet("me")]
    public async Task<ActionResult<AccountResponse>> GetCurrentAccount(CancellationToken cancellationToken) =>
        Ok(await accountService.GetCurrentAsync(cancellationToken));

    [HttpPut("me")]
    public async Task<ActionResult<AccountResponse>> UpdateCurrentAccount(
        UpdateAccountRequest request,
        CancellationToken cancellationToken) =>
        Ok(await accountService.UpdateCurrentAsync(request, cancellationToken));
}
