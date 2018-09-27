using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSR.PL.Web.AuthorizationAttributes;

namespace SSR.PL.Web.Controllers
{
    public class LibraryController : Controller
    {
        [HttpGet]
        [RoleAuthorize("Administrator",AuthenticationSchemes = "Identity.Application,Identity.External")]
        [Authorize(AuthenticationSchemes = "Identity.Application,Identity.External", Policy = "AdministratorUsers")]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
