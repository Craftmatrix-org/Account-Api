using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Craftmatrix.org.Services;
using Craftmatrix.org.DB;

namespace Craftmatrix.org.Controller
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RegisterController : ControllerBase
    {
        IPostgresService _db;
        public RegisterController(IPostgresService db)
        {
            _db = db;
        }
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] Whoami aim)
        {
            var allUser = await _db.GetAllAsync<Whoami>("WhoAmI");


            var thatOneWhorequest = allUser.Where(si => si.Email == aim.Email || si.Phone == aim.Phone);

            if (thatOneWhorequest.Count() == 0)
            {
                Guid GeneratedId = Guid.NewGuid();

                UserDto user = new UserDto();
                Whoami whoami = new Whoami();
                // whoami = aim;
                user.Id = GeneratedId;
                user.Joined = DateTime.UtcNow;
                whoami.UpdatedAt = DateTime.UtcNow;

                whoami.Email = aim.Email;
                whoami.Phone = aim.Phone;
                whoami.Password = aim.Password;

                try
                {
                    //await _db.PostDataAsync<Whoami>(whoami, "WhoAmI");
                    await _db.PostDataAsync<UserDto>(user, "User");


                    return Ok("explode");
                }
                catch (Exception ex)
                {
                    return Ok(ex.StackTrace);
                }
            }
            else
            {
                return BadRequest("Email or Phone Already Exist");
            }


        }
    }
}
