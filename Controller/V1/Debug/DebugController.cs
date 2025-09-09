using Craftmatrix.org.Services;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Craftmatrix.org.Helpers;
using Microsoft.AspNetCore.Authentication;
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
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Platinum_Task()
        {
            var token = JWTHelper.GenerateToken("", "", "", "", 60);
            var ds = _db.DebugString();
            return Ok(token);
        }
    }
}
