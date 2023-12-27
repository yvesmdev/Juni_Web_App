using Juni_Web_App.Models;
using Juni_Web_App.Models.Db;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using System.IO;

namespace Juni_Web_App.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()//user name and password received
        {
            return View();

        }

        public IActionResult SalesDashboard()
        {
            

                List<Order> OrderList = DatabaseRepository.GetAllSoldOrder();
                ViewBag.OrderList = OrderList;
                ViewBag.TotalRevenue = DatabaseRepository.GetTotalRevenue();
                int visitCount = DatabaseRepository.GetVisitCount();
                ViewBag.VisitCount = visitCount;
                ViewBag.ClientCount = DatabaseRepository.GetClientCount();
                ViewBag.OrderCompleteCount = DatabaseRepository.GetTotalCompleted();
                ViewBag.SalesCount = DatabaseRepository.GetSalesCount();

                List<Order> IncompleteOrderList = DatabaseRepository.GetAllOrderIncomplete();
                ViewBag.IncompleteOrderList = IncompleteOrderList;

                return View();
            
        }

        public IActionResult InventoryDashboard()
        {

            //HttpContext.Session.SetString("user", JsonConvert.SerializeObject(CurUser));//save session variable                
            /*
            string userString = HttpContext.Session.GetString("user");
            if(userString == null)
            {
                return RedirectToAction("Admin", "Login");
            }

            User CurUser = JsonConvert.DeserializeObject<User>(userString);//get user details
            ViewBag.CurUser = CurUser;*/

            List<ProductCategory> CategoryList = DatabaseRepository.GetProductCategories();//get product category list
            List<Product> ProductList = DatabaseRepository.GetProductList();//get product list

            
            ViewBag.ActiveProductsCount = DatabaseRepository.GetActiveProductCount();
            ViewBag.ArchivedProductsCount = DatabaseRepository.GetArchivedProductCount();
            ViewBag.UnderStockProductsCount = DatabaseRepository.GetUnderStockProductCount(5);
            ViewBag.ProductCategoryList = CategoryList;//get product category list
            ViewBag.ProductList = ProductList;

            List<Order> IncompleteOrderList = DatabaseRepository.GetAllOrderIncomplete();
            ViewBag.IncompleteOrderList = IncompleteOrderList;
            /*ViewBag.File1 = TempData["file1"];
            ViewBag.File2 = TempData["file2"];
            ViewBag.File3 = TempData["file3"];*/

            return View();
        }

        //Create Product
        [HttpPost]
        public IActionResult CreateProduct()
        {
            Product CurProduct = new Product();
            CurProduct.Name = Request.Form["productName"];
            CurProduct.CategoryId = Int32.Parse(Request.Form["productCategory"]);
            CurProduct.Qty = Int32.Parse(Request.Form["productQuantity"]);
            CurProduct.Price = Request.Form["productPrice"];
            CurProduct.Description = Request.Form["productDescription"];
            
            int productID = DatabaseRepository.AddProduct(CurProduct);//Add Product to Database
            if (productID != -1)
            {
                DatabaseRepository.AddProductImages(productID, Request.Form.Files);

            }
            return RedirectToAction("InventoryDashboard", "Admin");//Redirect to Inventory
        }

        //Update Product
        [HttpPost]
        public IActionResult UpdateProduct()
        {
            Product CurProduct = new Product();
            CurProduct.id = Convert.ToInt32(Request.Form["productShowHiddenProductId"]);
            CurProduct.Name = Request.Form["productShowName"];
            CurProduct.CategoryId = Int32.Parse(Request.Form["productShowCategory"]);
            CurProduct.Qty = Int32.Parse(Request.Form["productShowQuantity"]);
            CurProduct.Price = Request.Form["productShowPrice"];
            CurProduct.Description = Request.Form["productShowDescription"];
            string product1UpdateFileName = Request.Form["showImage1_upload"];
            string product2UpdateFileName = Request.Form["showImage2_upload"];
            string product3UpdateFileName = Request.Form["showImage3_upload"];
            /*
            TempData["file1"] = product1UpdateFileName;
            TempData["file2"] = product2UpdateFileName;
            TempData["file3"] = product3UpdateFileName;
            */
            string contentToWrite;// = product1UpdateFileName+"|"+ product2UpdateFileName+"|"+ product3UpdateFileName;

            if (product1UpdateFileName != null)
            {
                contentToWrite = product1UpdateFileName;
                using (StreamWriter streamWriter = new StreamWriter("temp.txt"))
                {
                    streamWriter.Write(contentToWrite);
                }
            }
            

            DatabaseRepository.UpdateProduct(CurProduct);//Update Product to Database            
            //Update Images
            if(product1UpdateFileName != null) {
                DatabaseRepository.UpdateProductImage(CurProduct.id,1, product1UpdateFileName, Request.Form.Files);
            }
            if (product2UpdateFileName != null)
            {
                DatabaseRepository.UpdateProductImage(CurProduct.id, 2, product2UpdateFileName, Request.Form.Files);
            }
            if (product3UpdateFileName != null)
            {
                DatabaseRepository.UpdateProductImage(CurProduct.id, 3, product3UpdateFileName, Request.Form.Files);
            }
            return RedirectToAction("InventoryDashboard", "Admin");//Redirect to Inventory
        }

        //Login User
        //Create Product
        [HttpPost]
        public IActionResult LoginUser()
        {
            string email = Request.Form["admin_email"];
            string password = Request.Form["admin_password"];
            bool check = DatabaseRepository.IsUserAuthorised(email, password, (int)UserRole.Admin);

            if (check)
            {
                
                User CurUser = DatabaseRepository.GetUserByUsername(email);
                //HttpContext.Session.SetString("user", JsonConvert.SerializeObject(CurUser));//save session variable                
                return RedirectToAction("InventoryDashboard", "Admin");//Redirect to Inventory
            }
            else
            {
                TempData["login"] = false;
                return RedirectToAction("Admin", "Login");//return to login
            }
            
        }

        public IActionResult OrderDashboard()
        {
            List<Order> OrderList = DatabaseRepository.GetAllOrder();
            ViewBag.OrderList = OrderList;
            ViewBag.DeliverableCount = DatabaseRepository.GetTotalForDelivery();
            ViewBag.CollectionCount = DatabaseRepository.GetTotalForCollection();
            ViewBag.OrderCompleteCount = DatabaseRepository.GetTotalCompleted();

            List<Order> IncompleteOrderList = DatabaseRepository.GetAllOrderIncomplete();
            ViewBag.IncompleteOrderList = IncompleteOrderList;

            return View();
        }

        public IActionResult OrderCollectionDashboard()
        {
            List<Order> OrderList = DatabaseRepository.GetAllOrderForCollection();
            ViewBag.OrderList = OrderList;
            ViewBag.DeliverableCount = DatabaseRepository.GetTotalForDelivery();
            ViewBag.CollectionCount = DatabaseRepository.GetTotalForCollection();
            ViewBag.OrderCompleteCount = DatabaseRepository.GetTotalCompleted();

            List<Order> IncompleteOrderList = DatabaseRepository.GetAllOrderIncomplete();
            ViewBag.IncompleteOrderList = IncompleteOrderList;

            return View();
        }


        public IActionResult OrderDeliveryDashboard()
        {
            List<Order> OrderList = DatabaseRepository.GetAllOrderForDelivery();
            ViewBag.OrderList = OrderList;
            ViewBag.DeliverableCount = DatabaseRepository.GetTotalForDelivery();
            ViewBag.CollectionCount = DatabaseRepository.GetTotalForCollection();
            ViewBag.OrderCompleteCount = DatabaseRepository.GetTotalCompleted();

            List<Order> IncompleteOrderList = DatabaseRepository.GetAllOrderIncomplete();
            ViewBag.IncompleteOrderList = IncompleteOrderList;

            return View();
        }




        /*
        public IActionResult Index(string username, string password)//user name and password received
        {
            if(username.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login","Login");
            }
            
        }*/
    }
}
