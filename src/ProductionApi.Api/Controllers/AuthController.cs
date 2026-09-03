using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductionApi.Application.Common.Interfaces;

namespace ProductionApi.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public sealed class AuthController(IJwtTokenService tokenService, IWebHostEnvironment environment) : ControllerBase
{
    /// <summary>
    /// Issues a signed token so the protected endpoints can be exercised locally.
    /// This is a stub, not an identity system - replace it with Entra ID, Auth0 or ASP.NET Core Identity.
    /// </summary>
    [HttpPost("dev-token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult IssueDevelopmentToken(DevTokenRequest request)
    {
        // Hidden outside development so a deployed instance can never mint tokens without credentials.
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var (accessToken, expiresAtUtc) = tokenService.CreateToken(
            Guid.CreateVersion7().ToString(),
            request.Email,
            ["User"]);

        return Ok(new { accessToken, expiresAtUtc });
    }
}

public sealed record DevTokenRequest([Required][EmailAddress] string Email);
