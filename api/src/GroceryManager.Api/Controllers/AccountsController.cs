using Microsoft.AspNetCore.Mvc;

namespace GroceryManager.Api.Controllers;

[ApiController]
[Route("api/accounts")]
public sealed class AccountsController : ControllerBase
{
    [HttpPost("register")]
    public IActionResult Register() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost("login")]
    public IActionResult Login() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost("logout")]
    public IActionResult Logout() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost("forgot-password")]
    public IActionResult ForgotPassword() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPost("reset-password")]
    public IActionResult ResetPassword() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpGet("me")]
    public IActionResult GetCurrentAccount() => StatusCode(StatusCodes.Status501NotImplemented);

    [HttpPut("me")]
    public IActionResult UpdateCurrentAccount() => StatusCode(StatusCodes.Status501NotImplemented);
}
