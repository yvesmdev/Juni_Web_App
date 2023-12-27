using Juni_Web_App.Models;
using Microsoft.AspNetCore.Mvc;

namespace Juni_Web_App.Controllers
{
    public class InventoryController : Controller
    {
        [HttpPost]
        public IActionResult CreateProduct(Product ProductModel)
        {
            return View();
        }
    }
}
