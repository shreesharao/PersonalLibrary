using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSR.PL.Web.AuthorizationAttributes;

namespace SSR.PL.Web.Controllers
{
    public class LibraryController : Controller
    {
        IAuthorizationService _authorizationService = null;
        public LibraryController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }
        [HttpGet]
        [RoleAuthorize("Administrator", AuthenticationSchemes = "Identity.Application,Identity.External")]
        [Authorize(AuthenticationSchemes = "Identity.Application,Identity.External", Policy = "AdministratorUsers")]
        public IActionResult Dashboard()
        {
            //RoleAdministrator is the policy name constructed in RoleAuthorizeAttribute class
            if (_authorizationService.AuthorizeAsync(User, "RoleAdministrator").Result.Succeeded)
            {
                return View();
            }
            return RedirectToAction("Login", "Profile");
        }
    }
}
