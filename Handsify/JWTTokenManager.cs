using Handsify.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace Handsify
{
    public class JwtTokenManager
    {
        public readonly double tokenExpiryTime = 15;
        private readonly IConfiguration _configuration;

        public JwtTokenManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Authenticate(LoggedInUserModel user)
        {
            // Get the JWT secret key from environment variables
            var key = Environment.GetEnvironmentVariable("JwtConfigKey");
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var tokenHandler = new JwtSecurityTokenHandler();

            // Initialize claims with basic user information
            var claims = new List<Claim>
    {
        new Claim("Name", user.Name.Trim()),
        new Claim("ADUser", user.ADUser.Trim())

    };
            foreach (string role in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            // Add each role as a separate claim using ClaimTypes.Role


            // Build the token descriptor with claims and signing credentials
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(tokenExpiryTime),
                Issuer = Environment.GetEnvironmentVariable("Issuer"),
                Audience = Environment.GetEnvironmentVariable("Audience"),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(keyBytes),
                    SecurityAlgorithms.HmacSha256Signature)
            };


            // Create and write the JWT token
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


    }
}
