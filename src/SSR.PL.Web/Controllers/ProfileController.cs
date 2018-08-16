using Microsoft.AspNetCore.Mvc;

namespace SSR.PL.Web.Controllers
{
    public class ProfileController:Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
