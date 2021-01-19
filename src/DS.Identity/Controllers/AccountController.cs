using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DS.Identity.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        [HttpGet]
        public IActionResult AuthCookieClaims()
        {
            return Ok(User.Claims
                .OrderBy(c => c.Type)
                .ToDictionary(c => c.Type, c => c.Value));
        }
    }
}