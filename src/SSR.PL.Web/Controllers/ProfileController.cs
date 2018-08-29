﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SSR.PL.Web.Entities;
using SSR.PL.Web.Models;
using SSR.PL.Web.Services.Abstractions;
using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SSR.PL.Web.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser<Guid>> _userManager;
        private readonly SignInManager<ApplicationUser<Guid>> _signInManager;
        private readonly ILogger _logger;
        private readonly IApplicationEmailSender _applicationEmailSender;

        public ProfileController(UserManager<ApplicationUser<Guid>> userManager, SignInManager<ApplicationUser<Guid>> signInManager, ILoggerFactory loggerFactory, IApplicationEmailSender applicationEmailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = loggerFactory.CreateLogger<ProfileController>();
            _applicationEmailSender = applicationEmailSender;
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

        //external login
        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider = "Google", string returnurl = null)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string returnurl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                _logger.LogCritical(remoteError);
                return RedirectToAction("Login", "Profile");
            }
            else
            {
                var result = await _signInManager.GetExternalLoginInfoAsync();

                if (result == null)
                {
                    _logger.LogCritical("Authentication failed for external provider");
                    return RedirectToAction("Login", "Profile");
                }
                else
                {
                    _logger.LogTrace($"Logged in with Provider :{result.ProviderDisplayName}");
                }
            }
            return RedirectToAction("Dashboard", "Library");
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
                user.Email = registerViewModel.Email;
                var result = await _userManager.CreateAsync(user, registerViewModel.Password);

                if (result.Succeeded)
                {
                    var emailConfirmationToken = _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Page(
                        pageName: $"/Profile/ConfirmEmail",
                        pageHandler: null,
                        values: new { userid = registerViewModel.Email, code = emailConfirmationToken },
                        protocol: Request.Scheme);

                    await _applicationEmailSender.SendEmailAsync(registerViewModel.Email, "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    //return RedirectToAction(nameof(ProfileController.Login), "Profile");
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
