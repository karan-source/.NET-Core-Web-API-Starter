namespace ProductionApi.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string AccessToken, DateTimeOffset ExpiresAtUtc) CreateToken(
        string userId,
        string email,
        IEnumerable<string> roles);
}
