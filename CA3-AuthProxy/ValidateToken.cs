using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
namespace CA3_AuthProxy;
public class ValidateToken
{
    private readonly ILogger<ValidateToken> _logger;
    private const string ExpectedIssuer = "https://player-auth.services.api.unity.com";
    public ValidateToken(ILogger<ValidateToken> logger)
    {
        _logger = logger;
    }
    [Function("ValidateToken")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
       _logger.LogInformation("ValidateToken invoked.");
        var token = req.Query["token"].ToString();
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("No token in request.");
            return BuildResponse(resultCode: 3, message: "no token provided");
        }
        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
        {
            _logger.LogWarning("Token is not a valid JWT structure.");
            return BuildResponse(resultCode: 2, message: "malformed token");
        }
        JwtSecurityToken jwt;
        try
        {
            jwt = handler.ReadJwtToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "JWT parse failed.");
            return BuildResponse(resultCode: 2, message: "could not parse token");
        }
        // Issuer check — must come from Unity Authentication.
        if (!string.Equals(jwt.Issuer, ExpectedIssuer, StringComparison.Ordinal))
        {
            _logger.LogWarning("Issuer mismatch: {Issuer}", jwt.Issuer);
            return BuildResponse(resultCode: 2, message: "wrong issuer");
        }
        // Expiry check.
        if (jwt.ValidTo == default || jwt.ValidTo < DateTime.UtcNow)
        {
            _logger.LogWarning("Token expired at {Exp}.", jwt.ValidTo);
            return BuildResponse(resultCode: 2, message: "token expired");
        }
        // Player id (sub claim).
        var playerId = jwt.Subject;
        if (string.IsNullOrWhiteSpace(playerId))
        {
            _logger.LogWarning("Token has no sub claim.");
            return BuildResponse(resultCode: 2, message: "missing player id");
        }
        _logger.LogInformation("Token accepted for PlayerId={PlayerId}", playerId);
        return BuildResponse(
            resultCode: 1,
            message: "validated",
            userId: playerId);
    }
    private static IActionResult BuildResponse(
        int resultCode, string message, string? userId = null)
    {
        return new OkObjectResult(new
        {
            ResultCode = resultCode,
            Message = message,
            UserId = userId
        });
    }
}