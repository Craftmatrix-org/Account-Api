using Craftmatrix.org.Services;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Craftmatrix.org.Helpers;
// using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Craftmatrix.org.Test;
using Craftmatrix.org.DB;

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

        [HttpGet("object_return")]
        public async Task<IActionResult> Objecter()
        {
            TestDto tdto = new TestDto();
            tdto.wow = "iyot";
            tdto.wiw = "dog";
            var thisistheoutput = await _db.DebugFunction<TestDto>(tdto);
            return Ok(thisistheoutput);
        }

        [Authorize]
        [HttpGet("test")]
        public async Task<IActionResult> Try()
        {
            UserDto user = new UserDto();
            user.Id = Guid.NewGuid();
            user.Joined = DateTime.UtcNow;
            var resulta = await _db.PostDataAsync<UserDto>(user, "User");

            return Ok(resulta);
        }
    }
}
