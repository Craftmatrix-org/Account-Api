using Craftmatrix.org.Services;
using Microsoft.AspNetCore.Mvc;

namespace Craftmatrix.org.Controller{
    [ApiController]
    [Route("api/[controller]")]
    public class DebugController : ControllerBase
    {
        private readonly IPostgresService _db;
        public DebugController(IPostgresService db)
        {
            _db = db;
        }
        [HttpGet("test")]
        public async Task<IActionResult> TestTask()
        {
            return Ok("ok");
        }
        [HttpGet]
        public async Task<IActionResult> Platinum_Task()            
        {
            var ds = _db.DebugString();
            return Ok(ds);
        }
    }
}