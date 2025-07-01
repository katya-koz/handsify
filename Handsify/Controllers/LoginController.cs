using Handsify.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Handsify.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller

    {

        private readonly ILogger<LoginController> _logger;
        //public readonly AuthController authController; 
        public LoginController(ILogger<LoginController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login()
        {
            var model = new LoginModel
            {
                Message = ""

            };

            return View(model);
        }


        /* // GET: Error
         [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
         public IActionResult Error()
         {
             return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
         }*/
    }
}
