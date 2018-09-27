using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SSR.PL.Web.Entities;
using SSR.PL.Web.Models;
using SSR.PL.Web.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SSR.PL.Web.Controllers
{
    [AllowAnonymous]
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
            //if the user is already signed in, redirect him to dashboard page
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Dashboard", "Library");
            }
            else
            {
                ViewBag.Title = "Login";
                return View();
            }

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

                    //set cookie
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name,loginViewModel.Email),
                        new Claim(ClaimTypes.Role,$"Administrator")
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ApplicationScheme);

                    var authProperties = new AuthenticationProperties();

                    //Make the cookie persisitent if the user wants to
                    if(loginViewModel.Rememberme)
                    {
                        authProperties.IsPersistent = true;
                    }
                    
                    await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

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
            var externalLoginInfo = await _signInManager.GetExternalLoginInfoAsync();
            if (externalLoginInfo == null)
            {
                _logger.LogCritical("Authentication failed for external provider");
                return RedirectToAction("Login", "Profile");
            }

            var email = externalLoginInfo.Principal.Claims.FirstOrDefault(claim =>
             {
                 return claim.Type.Contains($"emailaddress");
             }).Value;

            //add the user to the database
            //I was getting System.InvalidOperationException: User security stamp cannot be null when calling _userManager.AddLoginAsync.
            //After some digging I found that my newly created User-Entities did not have a SecurityStamp. 
            //And the asp.net default UserManager expects a SecurityStamp and wants to set it as a Claim in the ClaimsIdentity.
            var newUser = new ApplicationUser<Guid>(email)
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                Email = email

            };

            var existingUser = await _userManager.FindByEmailAsync(newUser.Email);

            if (existingUser?.Email == null)
            {
                var createUserResult = await _userManager.CreateAsync(newUser);

                if (!createUserResult.Succeeded)
                {
                    _logger.LogCritical($"Not able to create the user.Reason-{createUserResult}");
                    return RedirectToAction("Login", "Profile");
                }
            }

            var addLoginResult = await _userManager.AddLoginAsync(
                existingUser?.Email != null ? existingUser : newUser,
                new UserLoginInfo(externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey, externalLoginInfo.ProviderDisplayName));

            if (addLoginResult == null)
            {
                _logger.LogCritical("Not able to add external user to database");
                return RedirectToAction("Login", "Profile");
            }
            //sign in the user
            var signInResult = await _signInManager.ExternalLoginSignInAsync(externalLoginInfo.LoginProvider, externalLoginInfo.ProviderKey, true);

            if (signInResult.Succeeded)
            {
                //sign-in the user with Identity.External scheme

                var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Name,externalLoginInfo.Principal.Claims.FirstOrDefault().Subject.Name),
                        new Claim(ClaimTypes.Role,"Administrator")
                        
                    };

                var claimsIdentity = new ClaimsIdentity(claims, IdentityConstants.ExternalScheme);

                var authProperties = new AuthenticationProperties();

                await HttpContext.SignInAsync(IdentityConstants.ExternalScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                _logger.LogTrace($"Logged in with Provider :{externalLoginInfo.ProviderDisplayName}");

                return RedirectToAction("Dashboard", "Library");
            }

            _logger.LogCritical(signInResult.ToString());
            return RedirectToAction("Login", "Profile");
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
            //delete the cookie on logout
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            //logout from identity
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
