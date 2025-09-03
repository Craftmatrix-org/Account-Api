using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Craftmatrix.org.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisterController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Register()
        {
            return Ok("Wwow");            
        }
    }
}