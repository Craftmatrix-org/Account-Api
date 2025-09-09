using Craftmatrix.org.Services;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Craftmatrix.org.Helpers;
// using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace Craftmatrix.org.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly IPostgresService _db;

        public DebugController(IPostgresService db)
        {
            _db = db;
            DotNetEnv.Env.Load();
            DotNetEnv.Env.TraversePath().Load();
        }

        [HttpGet]
        public async Task<IActionResult> Platinum_Task()
        {

            var key = "";
            var issuer = "";
            var audience = "";
            var token = JWTHelper.GenerateToken("test", Guid.NewGuid(), 60);
            var ds = _db.DebugString();
            return Ok(token);
        }

        [Authorize]
        [HttpGet("test")]
        public async Task<IActionResult> Try()
        {
            return Ok("It works");
        }
    }
}
