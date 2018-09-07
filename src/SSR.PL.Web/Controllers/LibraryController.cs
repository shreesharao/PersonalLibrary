using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace SSR.PL.Web.Controllers
{
    public class LibraryController : Controller
    {
        [HttpGet]
        [Authorize(AuthenticationSchemes =  "Identity.Application", Roles = "Administrator")]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
