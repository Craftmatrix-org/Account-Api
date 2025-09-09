using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Craftmatrix.org.Helpers
{
    public class JWTHelper
    {
        public JWTHelper()
        {
            DotNetEnv.Env.Load();
            DotNetEnv.Env.TraversePath();
        }
        public static string GenerateToken(string username, Guid Id, int expireMinutes = 60)
        {

            string KEY = DotNetEnv.Env.GetString("KEY");
            string ISSUER = DotNetEnv.Env.GetString("ISSUER");
            string AUDIENCE = DotNetEnv.Env.GetString("AUDIENCE");


            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
              new Claim(JwtRegisteredClaimNames.Sub, username),
              new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: ISSUER,
                audience: AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
