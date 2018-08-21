using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace SSR.PL.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly ILogger _logger;
        public ProfileController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ProfileController>();
        }
        [HttpGet]
        public IActionResult Login()
        {
            ViewBag.Title = "Login";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login([Bind("email","password", "rememberme")]string email,string password,bool rememberme)
        {
            _logger.LogDebug(email);
           ViewBag.Title = "Login";
           return RedirectToAction("Dashboard", "Library");
        }
    }
}
