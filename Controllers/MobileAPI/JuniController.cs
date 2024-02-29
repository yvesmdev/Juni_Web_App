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

        //GET: api/juni/client_discount_perc 
        [HttpGet("get_client_discount_perc")]
        public string GetClientDiscountPerc()
        {
            return DatabaseRepository.GetClientDiscountPerc();
        }

        //GET: api/juni/client_discount_perc 
        [HttpGet("get_agent_commission_perc")]
        public string GetAgentCommisionPerc()
        {
            return DatabaseRepository.GetAgentCommissionPerc();
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

        //GET: api/juni/get_coupon_profile/{coupon}
        [HttpGet("get_coupon_profile/{coupon}")]
        public CouponProfile GetCouponProfile(string coupon)
        {
            return DatabaseRepository.GetCouponProfileById(coupon);
        }

        //GET: api/juni/get_coupon_profile/{coupon}
        [HttpGet("get_agent_market/{agentId}")]
        public CouponProfile GetAgentMarket(string agentId)
        {
            return DatabaseRepository.GetAgentMarketById(agentId);
        }

        //GET: api/juni/get_coupon_profile/{coupon}
        [HttpGet("get_agent_balance/{agentId}")]
        public string GetAgentBalance(string agentId)
        {
            return DatabaseRepository.GetAgentBalance(agentId)+"";
        }


        [HttpGet("get_agent_sales_list/{agentId}")]
        public List<Sale> GetAgentSalesList(string agentId)
        {
            return DatabaseRepository.GetAgentSaleList(agentId);// + "";
        }

        [HttpGet("get_client_order_list/{userId}")]
        public List<Sale> GetClientOrderList(string userId)
        {
            return DatabaseRepository.GetClientOrderList(userId);// + "";
        }


        [HttpGet("add_to_agent_market/{product_id}/{agent_id}")]
        public int AddToAgentMarket(string product_id, string agent_id)
        {
            return DatabaseRepository.AddProductToAgentMarket(product_id, agent_id);
        }

        [HttpGet("remove_from_agent_market/{product_id}/{agent_id}")]
        public int RemoveFromAgentMarket(string product_id, string agent_id)
        {
            return DatabaseRepository.RemoveProductFromAgentMarket(product_id, agent_id);
        }


        //[Route("api/juni/order")]
        [HttpPost("order")]
        [Consumes("application/json")]
        public string Order([FromBody] string jSonString)
        {
            try
            {
                Order ClientOrder = JsonConvert.DeserializeObject<Order>(jSonString);
                //DatabaseRepository.writeToFile("order.txt", ClientOrder.OrderUniqueId);                
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
                string countryCode = userData[1];
                string email = userData[2];
                string password = userData[3];

                int id = DatabaseRepository.AddUser(tel,countryCode,email, password);
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

        #region
        [HttpPost("agent_apply")]
        [Consumes("application/json")]
        public string CreateApplication([FromBody] string jSonString)
        {
            try
           {
                AgentApplication CurAgentApplication = JsonConvert.DeserializeObject<AgentApplication>(jSonString);
                DatabaseRepository.writeToFile("application.txt", CurAgentApplication.CellNumber);
                string response = DatabaseRepository.AddApplication(CurAgentApplication)+"";
                return response + "";
           }
            catch (Exception ex)
           {
                return $"[Error: {ex.Message}";
           }

        }
        #endregion


        //GET: api/juni/update_account/{username}/{password}
        [HttpGet("update_account/{cell}/{email}/{first_name}/{surname}")]
        public bool UpdateAccount(string cell, string email, string first_name, string surname)
        {
            try
            {
                DatabaseRepository.UpdateAccount(cell, email, first_name, surname);
                return true;
            }catch(Exception ex)
            {
                return false;
            }
        }

        //GET: api/juni/update_account/{username}/{password}
        [HttpGet("update_password/{cell}/{password}")]
        public bool UpdatePassword(string cell, string password)
        {
            try
            {
                DatabaseRepository.UpdatePassword(cell,password);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //GET: api/juni/update_account/{username}/{password}
        [HttpGet("reset_password/{cell}")]
        public bool ResetPassword(string cell)
        {
            try
            {
                DatabaseRepository.ResetPassword(cell,"+27");
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

    }



}
