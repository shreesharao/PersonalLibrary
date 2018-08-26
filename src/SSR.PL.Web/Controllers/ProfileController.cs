using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SSR.PL.Web.Entities;
using SSR.PL.Web.Models;
using System;
using System.Threading.Tasks;

namespace SSR.PL.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser<Guid>> _userManager;
        private readonly SignInManager<ApplicationUser<Guid>> _signInManager;
        private readonly ILogger _logger;

        public ProfileController(UserManager<ApplicationUser<Guid>> userManager, SignInManager<ApplicationUser<Guid>> signInManager, ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<ProfileController>();
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            ViewBag.Title = "Login";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(loginViewModel.Email, loginViewModel.Password, loginViewModel.Rememberme, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return RedirectToAction("Dashboard", "Library");
                }
                else
                {
                    _logger.LogError($"{result}");
                }
            }
            return View(loginViewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            ViewBag.Title = "Register";
            return View();
        }

        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser<Guid>(registerViewModel.Email);
                var result = await _userManager.CreateAsync(user, registerViewModel.Password);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(ProfileController.Login), "Profile");
                }
                else
                {
                    AddErrors(result);
                }
            }
            return View(registerViewModel);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogDebug("user logged out successfully");
            return RedirectToAction(nameof(ProfileController.Login), "Profile");
        }

        #region private methods
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
      
        #endregion
    }


}
