using Craftmatrix.org.Services;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Craftmatrix.org.Controller
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly IPostgresService _db;

        public DebugController(IPostgresService db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Platinum_Task()
        {
            var ds = _db.DebugString();
            return Ok(ds);
        }
    }
}
