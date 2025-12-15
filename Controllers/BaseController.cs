using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
public abstract class BaseController : ControllerBase
{
    protected int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst("id")?.Value
                          ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new Exception("İstifadəçi identifikatoru tapılmadı.");

        return int.Parse(userIdClaim);
    }
}
