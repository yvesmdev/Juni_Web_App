using Microsoft.AspNetCore.Mvc;

namespace Juni_Web_App.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

		public IActionResult Admin()
		{
            ViewBag.LoginSuccess = true;
            if (TempData["login"] != null)
            {
                ViewBag.LoginSuccess = false;
            }
			return View();
		}

	}
}
