using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SSR.PL.Web.Controllers
{
    public class LibraryController : Controller
    {
        [HttpGet]
        [Authorize(AuthenticationSchemes = "Identity.Application,Identity.External", Policy = "AdministratorUsers")]
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
