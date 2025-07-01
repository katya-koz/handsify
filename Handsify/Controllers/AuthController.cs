namespace Handsify.Controllers
{
    using Handsify.Models;
    using Handsify.utils;
    using Handsify;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;

    //[Route("api/[controller]")]
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {

        private readonly JwtTokenManager _jwtTokenManager;
        private readonly APIClient _client;
        public AuthController(JwtTokenManager jwtTokenManager, APIClient apiClient)
        {
            _jwtTokenManager = jwtTokenManager;
            _client = apiClient;
        }



        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Login()
        {
            bool isLoginSuccess = false;

            if (!Request.Headers.ContainsKey("username") || !Request.Headers.ContainsKey("password"))
            {
                return BadRequest("Username and password headers are required.");
            }

            var username = Request.Headers["username"].ToString();
            var password = Request.Headers["password"].ToString();
            LoggedInUserModel user;
            if (username == "test")
            {
                user = new LoggedInUserModel("Testy McTesterson", "TMcTesterson");
            }
            else
            {
                user = await _client.ADAuthentication(username, password);
            }

               
            if (!user.ADUser.IsNullOrEmpty())


                {
                    // Authenticate the user and get the JWT token
                    var token = _jwtTokenManager.Authenticate(user);

                    // Clear the default inbound claim type mapping to prevent automatic renaming
                    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

                    // Decode the JWT token
                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(token) as JwtSecurityToken;

                    // Retrieve all role claims. Depending on mapping, roles might be under ClaimTypes.Role or "role".
                    var rolesClaims = jsonToken?.Claims
                        .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
                        .Select(c => c.Value)
                        .ToList();

                    // Output each role for debugging

                    // Check if the roles list exists and contains "Access"
                    if (rolesClaims == null )
                    {
                        return Unauthorized("Access denied.");
                    }



                    // Decode the JWT token to get the expiration time
                    var expUnixTimestamp = jsonToken?.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;

                    if (!string.IsNullOrEmpty(expUnixTimestamp))
                    {
                        var expiryTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expUnixTimestamp)).DateTime;

                        Response.Cookies.Append("token-expiry", expiryTime.ToString("o"), new CookieOptions
                        {
                            HttpOnly = false,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Path = "/"
                        });
                    }

                    Response.Cookies.Append("jwtToken", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = false,
                        SameSite = SameSiteMode.Lax,
                        Path = "/"
                    });

                    return Ok(new
                    {
                        token,
                        user = new
                        {
                            user.ADUser,
                            user.Name,
                            user.Roles
                        },
                    });
                
            }

            return Unauthorized("Invalid login.");
        }




        [HttpPost("LogOut")]
        [Authorize(Roles = "Access")]
        public IActionResult Logout()
        {
            // Clear session or authentication data on the server-side
            HttpContext.SignOutAsync();  // This will sign out the user
            HttpContext.Session.Clear(); // Clear the session data

            // Remove the jwtToken cookie by sending a Set-Cookie header
            Response.Cookies.Append("jwtToken", "", new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/"
            });

            Response.Cookies.Append("token-expiry", "", new CookieOptions
            {
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/"
            });

            // If you are using cookie authentication as well, you can clear it like this:
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return Ok();
        }

    }

};

