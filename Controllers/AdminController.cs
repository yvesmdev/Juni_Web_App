﻿using Juni_Web.Models;
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
        private IActionResult redirectAuthentication()//quick user if access is unauthorised
        {
            string userJson = HttpContext.Session.GetString("user");
            if (userJson == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return null;
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();//clear session variable
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Index()//user name and password received
        {

            IActionResult CurView = redirectAuthentication();//Redirect View If Not Logged In
            if(CurView != null)
            {
                return CurView;
            }

            return View();

        }

        public IActionResult ApplicationDashboard()
        {
            IActionResult CurView = redirectAuthentication();//Redirect View If Not Logged In
            if (CurView != null)
            {
                return CurView;
            }

            List<AgentApplication> ApplicationList = DatabaseRepository.GetAgentApplicationList();
            //get order list
            List<Order> IncompleteOrderList = DatabaseRepository.GetAllOrderIncomplete();
            
            
            ViewBag.IncompleteOrderList = IncompleteOrderList;
            ViewBag.ApplicationList = ApplicationList;
            return View();
        }

        public IActionResult AccountDashboard()
        {
            IActionResult CurView = redirectAuthentication();//Redirect View If Not Logged In
            if (CurView != null)
            {
                return CurView;
            }

            //Get user list
            List<User> UserList = DatabaseRepository.GetUserList();
            List<User> ClientList = DatabaseRepository.GetUserListByType((int)UserRole.Client);
            List<User> AgentList = DatabaseRepository.GetUserListByType((int)UserRole.Agent);
            List<User> AdminList = DatabaseRepository.GetUserListByType((int)UserRole.Admin);

            ViewBag.UserList = UserList;
            ViewBag.ClientCount = ClientList.Count;
            ViewBag.AgentCount = AgentList.Count;
            ViewBag.AdminCount = AdminList.Count;
            //ViewBag.AgentList = AgentList;

            //get order list
            List<Order> IncompleteOrderList = DatabaseRepository.GetAllOrderIncomplete();
            ViewBag.IncompleteOrderList = IncompleteOrderList;

            return View();
        }

        public IActionResult AdminAccountDashboard()
        {
            IActionResult CurView = redirectAuthentication();//Redirect View If Not Logged In
            if (CurView != null)
            {
                return CurView;
            }

            //Get user list            
            List<User> ClientList = DatabaseRepository.GetUserListByType((int)UserRole.Client);
            List<User> AgentList = DatabaseRepository.GetUserListByType((int)UserRole.Agent);
            List<User> AdminList = DatabaseRepository.GetUserListByType((int)UserRole.Admin);

            ViewBag.UserList = AdminList;
            ViewBag.ClientCount = ClientList.Count;
            ViewBag.AgentCount = AgentList.Count;
            ViewBag.AdminCount = AdminList.Count;
            //ViewBag.AgentList = AgentList;

            //get order list
            List<Order> IncompleteOrderList = DatabaseRepository.GetAllOrderIncomplete();
            ViewBag.IncompleteOrderList = IncompleteOrderList;

            return View();
        }

        public IActionResult AgentAccountDashboard()
        {
            IActionResult CurView = redirectAuthentication();//Redirect View If Not Logged In
            if (CurView != null)
            {
                return CurView;
            }

            //Get user list
            List<User> ClientList = DatabaseRepository.GetUserListByType((int)UserRole.Client);
            List<User> AgentList = DatabaseRepository.GetUserListByType((int)UserRole.Agent);
            List<User> AdminList = DatabaseRepository.GetUserListByType((int)UserRole.Admin);

            ViewBag.UserList = AgentList;
            ViewBag.ClientCount = ClientList.Count;
            ViewBag.AgentCount = AgentList.Count;
            ViewBag.AdminCount = AdminList.Count;
            //ViewBag.AgentList = AgentList;

            //get order list
            List<Order> IncompleteOrderList = DatabaseRepository.GetAllOrderIncomplete();
            ViewBag.IncompleteOrderList = IncompleteOrderList;

            return View();
        }

        public IActionResult ClientAccountDashboard()
        {
            IActionResult CurView = redirectAuthentication();//Redirect View If Not Logged In
            if (CurView != null)
            {
                return CurView;
            }

            //Get user list

            List<User> ClientList = DatabaseRepository.GetUserListByType((int)UserRole.Client);
            List<User> AgentList = DatabaseRepository.GetUserListByType((int)UserRole.Agent);
            List<User> AdminList = DatabaseRepository.GetUserListByType((int)UserRole.Admin);
            ViewBag.UserList = ClientList;
            ViewBag.ClientCount = ClientList.Count;
            ViewBag.AgentCount = AgentList.Count;
            ViewBag.AdminCount = AdminList.Count;
            //ViewBag.AgentList = AgentList;

            //get order list
            List<Order> IncompleteOrderList = DatabaseRepository.GetAllOrderIncomplete();
            ViewBag.IncompleteOrderList = IncompleteOrderList;
            return View();
        }

        public IActionResult SalesDashboard()
        {
            IActionResult CurView = redirectAuthentication();//Redirect View If Not Logged In
            if (CurView != null)
            {
                return CurView;
            }

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
            IActionResult CurView = redirectAuthentication();//Redirect View If Not Logged In
            if (CurView != null)
            {
                return CurView;
            }

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
            IActionResult CurView = redirectAuthentication();//Redirect View If Not Logged In
            if (CurView != null)
            {
                return CurView;
            }

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

        //Approve Application
        [HttpPost]
        public IActionResult ApproveApplication()
        {
            IActionResult CurView = redirectAuthentication();//Redirect View If Not Logged In
            if (CurView != null)
            {
                return CurView;
            }

            string applicationId = Request.Form["applicationId"];
            int status = DatabaseRepository.ApproveApplication(applicationId);
            return RedirectToAction("ApplicationDashboard", "Admin");
        }

      
        //Update Product
        [HttpPost]
        public IActionResult UpdateProduct()
        {
            IActionResult CurView = redirectAuthentication();//Redirect View If Not Logged In
            if (CurView != null)
            {
                return CurView;
            }

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
            /*string contentToWrite;// = product1UpdateFileName+"|"+ product2UpdateFileName+"|"+ product3UpdateFileName;

            if (product1UpdateFileName != null)
            {
                contentToWrite = product1UpdateFileName;
                using (StreamWriter streamWriter = new StreamWriter("temp.txt"))
                {
                    streamWriter.Write(contentToWrite);
                }
            }
            */

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
            //redirectAuthentication();//Redirect View If Not Logged In

            string email = Request.Form["admin_email"];
            string password = Request.Form["admin_password"];
            bool check = DatabaseRepository.IsUserAuthorised(email, password, (int)UserRole.Admin);

            if (check)
            {
                string code_mfa = DatabaseRepository.GenerateRandomNumber();
                string messageOwner = $"<b>Rapport Juni</b><br/><br/>Votre code d'acces temporaire est {"<b>" + code_mfa + "</b>"}<br/><br/> {DatabaseRepository.WebUrl}";
                
                DatabaseRepository.UpdateCodeMFA(email, code_mfa);//update code mfa
                //SendEmail
                DatabaseRepository.SendEmailInBackground(new string[] {email}, "Juni - Admin: Code MFA", messageOwner);
                //User CurUser = DatabaseRepository.GetUserByUsername(email);
                //HttpContext.Session.SetString("user", JsonConvert.SerializeObject(CurUser));//save session variable
                TempData["login_email"] = email;
                return RedirectToAction("AdminMFA", "Login");//Redirect to Inventory
            }
            else
            {
                TempData["login"] = false;
                return RedirectToAction("Admin", "Login");//return to login
            }
            
        }

        //Login User
        //Create Product
        [HttpPost]
        public IActionResult LoginUserMFA()
        {
            //redirectAuthentication();//Redirect View If Not Logged In

            string email = Request.Form["admin_email"];
            string code_mfa = Request.Form["code_mfa"];
            //string password = Request.Form["admin_password"];

            bool check = DatabaseRepository.IsUserMFAAuthorised(email, code_mfa, (int)UserRole.Admin);//, (int)UserRole.Admin);
            if (check)
            {
                DatabaseRepository.UpdateCodeMFA(email, null);//clear code mfa
                //User CurUser = DatabaseRepository.GetUserByUsername(email);
                //HttpContext.Session.SetString("user", JsonConvert.SerializeObject(CurUser));//save session variable
                //Session["data"] = "email";
                //Sess
                User CurUser = DatabaseRepository.GetUserByUsername(email);//get user object
                string userJson = JsonConvert.SerializeObject(CurUser);
                HttpContext.Session.SetString("user",userJson);
                return RedirectToAction("InventoryDashboard", "Admin");//Redirect to Inventory
            }
            else
            {
                TempData["login_mfa"] = false;
                return RedirectToAction("AdminMFA", "Login");//return to login
            }

        }

        public IActionResult OrderDashboard()
        {
            IActionResult CurView = redirectAuthentication();//Redirect View If Not Logged In
            if (CurView != null)
            {
                return CurView;
            }

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
            IActionResult CurView = redirectAuthentication();//Redirect View If Not Logged In
            if (CurView != null)
            {
                return CurView;
            }

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
            IActionResult CurView = redirectAuthentication();//Redirect View If Not Logged In
            if (CurView != null)
            {
                return CurView;
            }

            List<Order> OrderList = DatabaseRepository.GetAllOrderForDelivery();
            ViewBag.OrderList = OrderList;
            ViewBag.DeliverableCount = DatabaseRepository.GetTotalForDelivery();
            ViewBag.CollectionCount = DatabaseRepository.GetTotalForCollection();
            ViewBag.OrderCompleteCount = DatabaseRepository.GetTotalCompleted();
            List<Order> IncompleteOrderList = DatabaseRepository.GetAllOrderIncomplete();
            ViewBag.IncompleteOrderList = IncompleteOrderList;

            return View();
        }
        [HttpGet]
        public IActionResult ApproveOrder()
        {
            IActionResult CurView = redirectAuthentication();//Redirect View If Not Logged In
            if (CurView != null)
            {
                return CurView;
            }

            string orderId = Request.Query["order_id"];//get orderID
            Order ClientOrder = DatabaseRepository.GetOrderById(orderId);//get client order

            DatabaseRepository.ApproveOrder(ClientOrder,null);

            if(ClientOrder.OrderType == (int)OrderType.ProductDelivery)
            {
                return RedirectToAction("OrderDeliveryDashboard", "Admin");//Redirect to All Orders
            }
            else if (ClientOrder.OrderType == (int)OrderType.CreditCardDelivery)
            {
                return RedirectToAction("OrderDeliveryDashboard", "Admin");//Redirect to All Orders
            }
            else if (ClientOrder.OrderType == (int)OrderType.CreditCardCollection)
            {
                return RedirectToAction("OrderCollectionDashboard", "Admin");//Redirect to All Orders
            }
            else if (ClientOrder.OrderType == (int)OrderType.ProductCollection)
            {
                return RedirectToAction("OrderCollectionDashboard", "Admin");//Redirect to All Orders
            }
                       
            return RedirectToAction("OrderDashboard", "Admin");//Redirect to All Orders
        }
    }
}
