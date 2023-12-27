using Juni_Web_App.Models;
using Juni_Web_App.Models.Db;
using Juni_Web_App.Models.Mobile;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Juni_Web_App.Controllers.MobileAPI
{

    [Route("api/[controller]")]
    [ApiController]
    public class JuniController : ControllerBase
    {
        //GET: api/profile
        [HttpGet]
        public List<Person> GetProfiles()
        {
           return DatabaseRepository.getProfiles();
        }
        //GET: api/juni/{id}
        [HttpGet("{id}")]
        public Person GetProfile(int id)
        {
            return DatabaseRepository.getProfiles()[0];
        }

        //GET: api/juni/authenticate/{username}/{password}
        [HttpGet("authenticate/{username}/{password}")]
        public bool CheckProfile(string username,  string password)
        {            
            return DatabaseRepository.IsUserAuthorised(username,password);//authenticate user
        }

        //GET: api/juni/products
        [HttpGet("products")]
        public List<Product> GetProducts()
        {
            return DatabaseRepository.GetProductList();
        }

        
        //GET: api/juni/products
        [HttpGet("products/{id}")]
        public Product GetProductById(string id)
        {
            return DatabaseRepository.GetProductById(id);
        }

        //GET: api/juni/delivery_fee
        [HttpGet("delivery_fee")]
        public string GetDeliveryFee()
        {
            return DatabaseRepository.GetDeliveryFee();
        }

        //GET: api/juni/generate_order_id/{order_type}
        [HttpGet("generate_order_id/{order_type}")]
        public string GenerateOrderID(string order_type)
        {
            return DatabaseRepository.GetOrderUniqueID(int.Parse(order_type));
        }
        //GET: api/juni/get_user_id/{username}
        [HttpGet("get_user_id/{username}")]
        public User GetUserId(string username)
        {
            return DatabaseRepository.GetUserByUsername(username);
        }

        //POST: api/juni/order
        //[Route("api/juni/order")]
        //[HttpPost]
        /*public IActionResult Order([FromBody] string jSonString)
        {
            try
            {
                DatabaseRepository.writeToFile("order.txt", jSonString);
                Order ClientOrder = JsonConvert.DeserializeObject<Order>(jSonString);
                string response = DatabaseRepository.AddOrder(ClientOrder) + "";
                return Ok(response);

            }
            catch (Exception ex)
            {
                return BadRequest($"[Error: {ex.Message}" );
            }
            
        }*/
        //[Route("api/juni/order")]
        [HttpPost("order")]
        [Consumes("application/json")]
        public string Order([FromBody] string jSonString)
        {
            try
            {
                Order ClientOrder = JsonConvert.DeserializeObject<Order>(jSonString);
                DatabaseRepository.writeToFile("order.txt", ClientOrder.OrderUniqueId);                
                string response = DatabaseRepository.AddOrder(ClientOrder) + "";
                return response + "";              

            }
            catch (Exception ex)
            {
                return $"[Error: {ex.Message}";
            }

        }

        [HttpPost("create_user")]
        [Consumes("application/json")]
        public string CreateUser([FromBody] string jSonString)
        {
            try
            {
                string[] userData = jSonString.Split(";");
                string tel = userData[0];
                string email = userData[1];
                string password = userData[2];

                int id = DatabaseRepository.AddUser(tel,email, password);
                //Order ClientOrder = JsonConvert.DeserializeObject<Order>(jSonString);
                //DatabaseRepository.writeToFile("test.txt", jSonString);
                if(id <= 0)
                {
                    return "False";
                }
                else
                {
                    return "True";
                }
                //string response = DatabaseRepository.AddOrder(ClientOrder) + "";
                //return jSonString + "";

            }
            catch (Exception ex)
            {
                return $"[Error: {ex.Message}";
            }

        }

        public class MyModel
        {
            public MyModel(string mName)
            {
                this.Name = mName;
            }
            public string Name { get; set; }
        }
    }
}
