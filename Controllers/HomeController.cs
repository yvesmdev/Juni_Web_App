using Juni_Web_App.Models;
using Juni_Web_App.Models.Db;
using Juni_Web_App.Models.Mobile;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace Juni_Web_App.Controllers { 

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            List<Person> PersonList = DatabaseRepository.getProfiles();//get list of profiles
            ViewBag.Name = PersonList[0].user_name;
            return View();
        }

        [HttpGet]
        public IActionResult Product()
        {
            string product_id = Request.Query["product_id"];
            string coupon_id = Request.Query["coupon_od"];

            if (product_id != null)
            {
                DatabaseRepository.GetProductById(product_id);
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}