using Juni_Web.Models;
using Juni_Web_App.Models.Mobile;
using MySql.Data.MySqlClient;
using System.Data;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Juni_Web_App.Models.Db
{
    public class DatabaseRepository
    {


        public static string ConnectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["DefaultConnection"];

        public static string TwilioAccountSid = "AC754bf25d79d2c69f53bc21ff7f4cb2e5";
        public static string TwilioAuthToken = "e21834908216bb118969063213bc491f";
        public static string TwilioPhoneNumber = "+27670078670";
        public static string WebUrl = "https://juni-ecommerce.azurewebsites.net/";


        private static string MailSenderEmail = "notifications.noreply.bansosoftwares@bansoco.com"; // Your email address
        private static string MailSenderPassword = "giftedByGrace"; // Your email password
        private static string MailStmpDomain = "mail.bansoco.com"; //; "gauntlet.aserv.co.za";
        private static int MailStmpPort = 587; //;465;

        private static CultureInfo culture = CultureInfo.InvariantCulture;
        #region 
        //mesaging

        static void SendEmailInBackground(string[] recipientEmail,string subject, string body)
        {
            // Configure SMTP client for cPanel webmail
            SmtpClient smtpClient = new SmtpClient(MailStmpDomain);
            smtpClient.Port = MailStmpPort; // Port may vary, typically 587 for TLS/STARTTLS
            smtpClient.Credentials = new NetworkCredential(MailSenderEmail, MailSenderPassword);
            smtpClient.EnableSsl = false; // Enable SSL/TLS encryption
                                          //smtpClient.UseDefaultCredentials = false;
                                          // Create a MailMessage object
            MailMessage mail = new MailMessage(MailSenderEmail, recipientEmail[0]);
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true; // Set to true to enable HTML formatting


            for (int i = 1; i < recipientEmail.Length; i++)
            {
                mail.To.Add(recipientEmail[i]);
            }

            // Create and start a new thread to send the email
            Thread emailThread = new Thread(() =>
            {
                try
                {
                    // Send the email
                    smtpClient.Send(mail);
                    Console.WriteLine("Email sent successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to send email. Error: " + ex.Message);
                }
            });

            emailThread.Start();

            // Wait for the email sending thread to complete (optional)
            // emailThread.Join();

            // Optionally continue with other operations after the email is sent
        }

        public static void SendWhatsAppMessage(string receiverPhoneNumber, string messageBody)
        {
            // Send WhatsApp message in a separate thread
            Thread sendThread = new Thread(() =>
            {
                try
                {
                    TwilioClient.Init(TwilioAccountSid, TwilioAuthToken);
                    // Send WhatsApp message
                    var message = MessageResource.Create(
                        to: new PhoneNumber($"whatsapp:{receiverPhoneNumber}"),
                        from: new PhoneNumber($"whatsapp:{TwilioPhoneNumber}"),
                        body: messageBody
                    );
                    Console.WriteLine($"Message sent successfully! SID: {message.Sid}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send message: {ex.Message}");
                }
            });
            sendThread.Start();
        }

        #endregion

        //
        public static void writeToFile(string filename, string content)
        {
            using (StreamWriter streamWriter = new StreamWriter(filename))
            {
                streamWriter.Write(content);
            }
        }
        //Product

        #region

        public static int AddProduct(Product product)
        {
            if (product == null)
            {
                return -1;
            }

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();

                string Query = "INSERT INTO product(name,category_id,price,quantity,description) VALUES(@name,@categoryID,@price,@quantity,@desc); SELECT LAST_INSERT_ID()";
                MySqlCommand DbCommand = new MySqlCommand(Query, DbCon);
                DbCommand.Parameters.AddWithValue("@name", product.Name);
                DbCommand.Parameters.AddWithValue("@categoryID", product.CategoryId);
                DbCommand.Parameters.AddWithValue("@price", product.Price);
                DbCommand.Parameters.AddWithValue("@quantity", product.Qty);
                DbCommand.Parameters.AddWithValue("@desc", product.Description);
                
                int productID = Convert.ToInt32(DbCommand.ExecuteScalar());//fetch the productID use it to rename image files                    
                DbCon.Close();

                return productID;
            }

        }

        public static bool AddProductImages(int productID, IFormFileCollection files)
        {
            int count = 0;
            foreach (IFormFile file in files) {
                int id = file.FileName.IndexOf(".");
                string newName = productID+"_"+(++count)+file.FileName.Substring(id);

                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img","product", newName);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    file.CopyTo(stream);
                    using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
                    {
                        DbCon.Open();
                        string Query = "INSERT INTO product_image(path,product_id) VALUES(@path,@productID)";
                        MySqlCommand DbCommand = new MySqlCommand(Query, DbCon);
                        DbCommand.Parameters.AddWithValue("@path", "img/product/" + newName);
                        DbCommand.Parameters.AddWithValue("@productID", productID);
                        DbCommand.ExecuteNonQuery();
                        DbCon.Close();
                    }
                }              
            }
            return true;
        }


        public static int UpdateProduct(Product product)
        {
            if (product == null)
            {
                return -1;
            }

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();

                string Query = "UPDATE product SET name=@name,category_id=@categoryID,price=@price,quantity=@quantity,description=@desc WHERE product_id=@productID;SELECT LAST_INSERT_ID();";
                MySqlCommand DbCommand = new MySqlCommand(Query, DbCon);
                DbCommand.Parameters.AddWithValue("@productID", product.id);
                DbCommand.Parameters.AddWithValue("@name", product.Name);
                DbCommand.Parameters.AddWithValue("@categoryID", product.CategoryId);
                DbCommand.Parameters.AddWithValue("@price", product.Price.Replace(',','.'));
                DbCommand.Parameters.AddWithValue("@quantity", product.Qty);
                DbCommand.Parameters.AddWithValue("@desc", product.Description);

                int productID = Convert.ToInt32(DbCommand.ExecuteScalar());//fetch the productID use it to rename image files                    
                DbCon.Close();

                return productID;
            }

        }

        public static bool UpdateProductImage(int productID, int imageCountId, string imageName, IFormFileCollection files)
        {
            
            foreach (IFormFile file in files)
            {
                int id = file.FileName.IndexOf(".");
                var rawPath = productID + "_" + (imageCountId);
                string newName = rawPath + file.FileName.Substring(id);
                var relPath = "img/product/" + newName;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relPath);

                if (file.FileName.Equals(imageName))//if the file is found!!!
                {
                    MySqlConnection DbCon;
                    //check if product is not in the database already
                    using (DbCon = new MySqlConnection(ConnectionString))
                    {
                        DbCon.Open();
                        string Query = "SELECT path, image_id FROM product_image WHERE path LIKE 'img/product/" + rawPath+".%'";
                        MySqlCommand DbCommand = new MySqlCommand(Query, DbCon);
                        MySqlDataReader DbReader = DbCommand.ExecuteReader();
                        

                        if (DbReader.Read())//check existing image and delete it
                        {
                            string currentPath = (string)DbReader["path"];//find image and delete it
                            int imageId = Convert.ToInt32(DbReader["image_id"]);
                            var currentAbsolutePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", currentPath);

                            if (File.Exists(currentAbsolutePath))
                            {
                                File.Delete(currentAbsolutePath);//delete the image
                                using (MySqlConnection DbCon2 = new MySqlConnection(ConnectionString))
                                {
                                    DbCon2.Open();
                                    Query = "DELETE FROM product_image WHERE image_id=" + imageId;
                                    MySqlCommand DbCommand2 = new MySqlCommand(Query, DbCon2);
                                    DbCommand2.ExecuteScalar();//delete the image from the database
                                    DbCon2.Close();
                                }                                
                            }
                        }

                        DbCon.Close(); 

                        //copy new image
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            file.CopyTo(stream);
                            using (DbCon = new MySqlConnection(ConnectionString))
                            {
                                DbCon.Open();
                                Query = "INSERT INTO product_image(path,product_id) VALUES(@path,@productID)";
                                DbCommand = new MySqlCommand(Query, DbCon);
                                DbCommand.Parameters.AddWithValue("@path", "img/product/" + newName);
                                DbCommand.Parameters.AddWithValue("@productID", productID);
                                DbCommand.ExecuteNonQuery();
                                DbCon.Close();
                            }
                        }


                    }

                    return true;//leave the loop
                }               
            
            }
            return false;
        }


        public static List<AgentApplication> GetAgentApplicationList()
        {
            var ApplicationList = new List<AgentApplication>();

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM agent_application LEFT JOIN application_docs ON " +
                    " agent_application.id = application_docs.app_id ORDER BY agent_application.id DESC", DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    AgentApplication CurApplication = new AgentApplication();
                    CurApplication.id = Convert.ToInt32(DbReader["id"]);
                    CurApplication.CellNumber = (string)DbReader["tel"];
                    CurApplication.Name = (string)DbReader["full_name"];
                    CurApplication.Email = (string)DbReader["email"];
                    CurApplication.StreetAddress = (string)DbReader["street_address"];
                    CurApplication.Suburb = (string)DbReader["suburb"];
                    CurApplication.Municipality = (string)DbReader["municipality"];
                    CurApplication.City = (string)DbReader["city"];
                    CurApplication.Province = (string)DbReader["province"];
                    CurApplication.IdFileName = (string)DbReader["path"];
                    CurApplication.IsApproved = Convert.ToUInt64(DbReader["application_approved"]) > 0 ? true : false;
                    CurApplication.Date = DbReader.GetDateTime(DbReader.GetOrdinal("application_date")).ToString("yyyy-MM-dd");
                    ApplicationList.Add(CurApplication);
                }
                DbCon.Close();
            }
            return ApplicationList;
        }


        public static AgentApplication GetAgentApplicationById(string id)
        {
            var CurApplication = new AgentApplication();

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM agent_application LEFT JOIN application_docs ON " +
                    "agent_application.id = application_docs.app_id WHERE id="+id, DbCon);
                DatabaseRepository.writeToFile("sql.txt", DbCommand.CommandText);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    CurApplication = new AgentApplication();
                    CurApplication.id = Convert.ToInt32(DbReader["id"]);
                    CurApplication.CellNumber = (string)DbReader["tel"];
                    CurApplication.Name = (string)DbReader["full_name"];
                    CurApplication.Email = (string)DbReader["email"];
                    CurApplication.StreetAddress = (string)DbReader["street_address"];
                    CurApplication.Suburb = (string)DbReader["suburb"];
                    CurApplication.Municipality = (string)DbReader["municipality"];
                    CurApplication.City = (string)DbReader["city"];
                    CurApplication.Province = (string)DbReader["province"];
                    CurApplication.IdFileName = (string)DbReader["path"];
                    CurApplication.IsApproved = Convert.ToUInt64(DbReader["application_approved"]) > 0 ? true : false;
                    CurApplication.Date = DbReader.GetDateTime(DbReader.GetOrdinal("application_date")).ToString("yyyy-MM-dd");
                    
                }
                DbCon.Close();
            }
            return CurApplication;
        }


        public static List<Product> GetProductList()
        {
            var ProductList = new List<Product>();

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM product ORDER BY product_id DESC", DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    Product CurProduct = new Product();
                    CurProduct.id = Convert.ToInt32(DbReader["product_id"]);
                    CurProduct.Name = (string)DbReader["name"];
                    CurProduct.Description = (string)DbReader["description"];
                    CurProduct.Price = ""+DbReader["price"];
                    CurProduct.Qty = Convert.ToInt32(DbReader["quantity"]);
                    CurProduct.CategoryId = Convert.ToInt32(DbReader["category_id"]);
                    CurProduct.PreviewImagePaths = GetProductImagePaths(CurProduct.id);//Get Product Image Paths
                    ProductList.Add(CurProduct);
                }
                DbCon.Close();
            }
            return ProductList;

        }

        public static Product GetProductById(string id)
        {
            Product CurProduct = null;

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM product WHERE product_id="+id, DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    CurProduct = new Product();
                    CurProduct.id = Convert.ToInt32(DbReader["product_id"]);
                    CurProduct.Name = (string)DbReader["name"];
                    CurProduct.Description = (string)DbReader["description"];
                    CurProduct.Price = "" + DbReader["price"];
                    CurProduct.Qty = Convert.ToInt32(DbReader["quantity"]);
                    CurProduct.CategoryId = Convert.ToInt32(DbReader["category_id"]);
                    CurProduct.PreviewImagePaths = GetProductImagePaths(CurProduct.id);//Get Product Image Paths                    
                }
                DbCon.Close();
            }
            return CurProduct;
        }
        public static List<string> GetProductImagePaths(int productID)
        {
            List<string> ProductImagePaths = new List<string>();
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT path, SUBSTR(path, instr(path,'_')+1, instr(path,'.')-instr(path,'_')-1) AS prd_order FROM product_image  WHERE product_id=" + productID+ " ORDER BY prd_order ASC", DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    ProductImagePaths.Add((string)DbReader["path"]);  
                }
                DbCon.Close();
            }
            return ProductImagePaths;
        }

        public static int GetActiveProductCount()
        {
            var ProductList = new List<Product>();

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT COUNT(*) AS 'active_products' FROM product WHERE archived=0 ", DbCon);
                int activeProducts = 0;
                
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    activeProducts = Convert.ToInt32(DbReader["active_products"]);                   
                }
                DbCon.Close();

                return activeProducts;
            }
            
        }

        public static int GetArchivedProductCount()
        {
            var ProductList = new List<Product>();

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT COUNT(*) AS 'archived_products' FROM product WHERE archived != 0 ", DbCon);
                int activeProducts = 0;

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    activeProducts = Convert.ToInt32(DbReader["archived_products"]);
                }
                DbCon.Close();

                return activeProducts;
            }

        }

        public static int GetUnderStockProductCount(int threshold=5)
        {
            var ProductList = new List<Product>();

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT COUNT(*) AS 'understock_products' FROM product WHERE quantity < "+threshold, DbCon);
                int activeProducts = 0;

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    activeProducts = Convert.ToInt32(DbReader["understock_products"]);
                }
                DbCon.Close();

                return activeProducts;
            }

        }
        #endregion

        //Category

        public static List<ProductCategory> GetProductCategories()
        {
            var ProductCategoryList = new List<ProductCategory>();

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT category_id, title FROM product_category ORDER BY category_id DESC", DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    ProductCategory CurCategory = new ProductCategory();
                    CurCategory.Id = DbReader["category_id"].ToString();
                    CurCategory.Name = (string)DbReader["title"];
                    ProductCategoryList.Add(CurCategory);
                }
                DbCon.Close();
            }
            return ProductCategoryList;

        }
        //Profile API functions
        #region 
        public static bool IsUserAuthorised(string username, string password)
        {           
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM user_profile WHERE (" +
                    " username = '"+username+"' OR phone_number ='"+username+"' " +
                    "OR email ='"+username+"') AND (password='"+password+"')", DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    using (MySqlConnection DbCon2 = new MySqlConnection(ConnectionString))
                    {
                        DbCon2.Open();
                        //increment number of visits
                        string Query = "UPDATE configuration SET value = CONVERT(CONVERT(value, SIGNED INTEGER) + 1, CHAR) WHERE key_name = 'num_visits';";
                        MySqlCommand DbCommand2 = new MySqlCommand(Query, DbCon2);
                        DbCommand2.ExecuteScalar();
                        DbCon2.Close();
                    }
                    DbCon.Close();
                    return true;
                }
                else
                {
                    DbCon.Close();
                    return false;
                }                
            }
           
        }

        public static bool IsUserAuthorised(string username, string password, int userRoleId)
        {
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM user_profile WHERE (" +
                    " username = '" + username + "' OR phone_number ='" + username + "' " +
                    "OR email ='" + username + "') AND (password='" + password + "') AND (user_role_id="+userRoleId+")", DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    using (MySqlConnection DbCon2 = new MySqlConnection(ConnectionString))
                    {
                        DbCon2.Open();
                        //increment number of visits
                        string Query = "UPDATE configuration SET value = CONVERT(CONVERT(value, SIGNED INTEGER) + 1, CHAR) WHERE key_name = 'num_visits';";
                        MySqlCommand DbCommand2 = new MySqlCommand(Query, DbCon2);
                        DbCommand2.ExecuteScalar();
                        DbCon2.Close();
                    }
                    DbCon.Close();
                    return true;
                }
                else
                {
                    DbCon.Close();
                    return false;
                }
            }

        }

        public static List<Person> getProfiles()
        {
            List<Person> PersonList = new List<Person>();
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM user_profile", DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    Person person = new Person();
                    person.user_id = Convert.ToInt32(DbReader["user_id"]);
                    person.user_name = (string)DbReader["username"];
                    person.email = (string)DbReader["email"];
                    person.cell_number = (string)DbReader["phone_number"];
                    PersonList.Add(person);
                }
                DbCon.Close();
            }
            return PersonList;

        }
        #endregion

        //Emails
        public static string[] GetNotificationEmails()
        {
            string message = null;

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT value FROM configuration WHERE key_name='notification_mail'", DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    message = (string)DbReader["value"];
                }
                DbCon.Close();
            }

            string[] emailList = message.Split(';');
            return emailList;
        }

        //Emails
        public static string[] GetNotificationCells()
        {
            string message = null;

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT value FROM configuration WHERE key_name='notification_cell'", DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    message = (string)DbReader["value"];
                }
                DbCon.Close();
            }

            string[] cellList = message.Split(';');
            return cellList;
        }

        //Delivery

        public static string GetDeliveryFee()
        {
            string deliveryFee = null;

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT value FROM configuration WHERE key_name='delivery_fee'", DbCon);                

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    deliveryFee = (string)DbReader["value"];
                }
                DbCon.Close();                
            }
            return deliveryFee;
        }

        //Delivery
        public static string GetClientDiscountPerc()
        {
            string perc = null;

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT value FROM configuration WHERE key_name='client_coupon_discount_perc'", DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    perc = (string)DbReader["value"];
                }
                DbCon.Close();
            }
            return perc;
        }

        public static List<Sale> GetAgentSalesList(string agent_id)
        {

            User CurUser = GetUserByUsername(agent_id);
            if(CurUser == null)
            {
                return null;
            }

            DatabaseRepository.writeToFile("agentssales.txt", agent_id);

            List<Sale> SalesList = null;

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM order_table WHERE coupon_code='"+CurUser.coupon_code+"'", DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    string orderUniqId = (string)DbReader["order_unique_id"];
                    double commissionPerc = Convert.ToDouble(DbReader["agent_comission_perc"]);
                    string clientCell = (string)DbReader["sender_cell"];
                    bool isDiscounted = Convert.ToInt32(DbReader["is_discounted"]) > 0 ? true : false;
                    int orderId = Convert.ToInt32(DbReader["order_id"]);
                        
                    Sale CurSale = new Sale();
                    CurSale.OrderUniqueId = orderUniqId;
                    CurSale.commissionPerc = commissionPerc;
                    CurSale.ClientCell = clientCell;
                    CurSale.IsDiscounted = isDiscounted;
                    CurSale.OrderId = orderId;
                    CurSale.ProductList = new List<Product>();
                    using (MySqlConnection DbCon2 = new MySqlConnection(ConnectionString))
                    {
                        DbCon2.Open();
                        MySqlCommand DbCommand2 = new MySqlCommand("SELECT * FROM order_details WHERE order_id='" + orderId + "'", DbCon2);
                        MySqlDataReader DbReader2 = DbCommand.ExecuteReader();
                        while (DbReader2.Read())
                        {
                            
                            bool prdDiscounted = Convert.ToInt32(DbReader2["product_agent_discounted"]) > 0 ? true : false;

                            if (!prdDiscounted)
                            {
                                continue;
                            }

                            Product CurProduct = new Product();
                            CurProduct.id = Convert.ToInt32(DbReader2["product_id"]);
                            CurProduct = GetProductById(CurProduct.id+"");

                            double discount = Convert.ToDouble(DbReader2["product_agent_price_discount"]);
                            double price = Convert.ToDouble(DbReader2["product_price"]);
                            CurProduct.Price = (price + discount).ToString();

                            CurSale.ProductList.Add(CurProduct);
                        }
                        DbCon2.Close();
                    }

                    SalesList.Add(CurSale);

                }
                DbCon.Close();
            }
            return SalesList;

        }


        public static string GetAgentCommissionPerc()
        {
            string perc = null;

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT value FROM configuration WHERE key_name='agent_comission_perc'", DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    perc = (string)DbReader["value"];
                }
                DbCon.Close();
            }
            return perc;
        }

        //Generate Unique Order ID
        public static string GetOrderUniqueID(int orderType)
        {
            string  orderUniqueID = null;
            //Get the current date and time
            DateTime now = DateTime.Now;
            //Format the date and time as YYMMDDHHmm
            string formattedDateTime = now.ToString("yyMMddHHmm");
            string dateForSql = now.ToString("yyyy-MM-dd HH:MM");

            //
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT (COUNT(*)+1) AS num_orders FROM order_table WHERE DATE_FORMAT(order_date,'%Y-%m-%d %H:%i') = DATE_FORMAT('" + dateForSql+ "','%Y-%m-%d %H:%i')", DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    string orderNumber = Convert.ToInt32(DbReader["num_orders"]).ToString("D2");
                    string orderTypeStr;

                    if (orderType == 1)
                    {
                        orderTypeStr = "CR";//credit card collect
                    }
                    else if (orderType == 2)
                    {
                        orderTypeStr = "CL";//credit card delivery
                    }
                    else if (orderType == 3)
                    {
                        orderTypeStr = "RP";//retrieve product
                    }
                    else if (orderType == 4)
                    {
                        orderTypeStr = "LP";//cash on delivery
                    }
                    else{
                        orderTypeStr = "XX";//Inconny
                    }
                    orderUniqueID = orderTypeStr + formattedDateTime + orderNumber;
                }
                DbCon.Close();
            }

            return orderUniqueID;
        }

        public static bool AddOrder(Order ClientOrder)
        {
           
            bool orderSuccess = false;
            double total = 0;
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                
                DbCon.Open();
                
                double commission_perc = double.Parse(GetAgentCommissionPerc(), NumberStyles.Float, culture);
                string Query = "INSERT INTO order_table(customer_id,sender_fullname,sender_cell,dispatch_address,dest_fullname,dest_cell,dest_gift_message,order_date,order_type_id,deliveryFee,order_unique_id,completed,coupon_code,is_discounted,agent_comission_perc)" +
                    " VALUES(@customerID,@senderFullname,@senderCell,@dispatchAddress,@destFullname,@destCell,@destGiftMessage,@orderDate,@orderTypeId,@deliveryFee,@orderUniqueId,@completed,@couponCode,@isDiscounted,@commissionPerc); SELECT LAST_INSERT_ID()";

                MySqlCommand DbCommand = new MySqlCommand(Query, DbCon);
                DbCommand.Parameters.AddWithValue("@customerID",ClientOrder.ClientId);
                DbCommand.Parameters.AddWithValue("@senderFullname",ClientOrder.SenderFullname);
                DbCommand.Parameters.AddWithValue("@senderCell", ClientOrder.SenderCell);
                DbCommand.Parameters.AddWithValue("@dispatchAddress",ClientOrder.Address);
                DbCommand.Parameters.AddWithValue("@destFullname", ClientOrder.DestinatorFullname);
                DbCommand.Parameters.AddWithValue("@destCell", ClientOrder.DestinatorCell);
                DbCommand.Parameters.AddWithValue("@destGiftMessage", ClientOrder.GiftMessage);
                DbCommand.Parameters.AddWithValue("@orderDate", ClientOrder.OrderDate);
                DbCommand.Parameters.AddWithValue("@orderTypeId", ClientOrder.OrderType);
                DbCommand.Parameters.AddWithValue("@deliveryFee", ClientOrder.DeliveryFee);
                DbCommand.Parameters.AddWithValue("@orderUniqueId", ClientOrder.OrderUniqueId);
                DbCommand.Parameters.AddWithValue("@completed", ClientOrder.OrderCompleted);
                DbCommand.Parameters.AddWithValue("@couponCode", ClientOrder.CouponCode);
                DbCommand.Parameters.AddWithValue("@isDiscounted", ClientOrder.IsDiscounted);
                DbCommand.Parameters.AddWithValue("@commissionPerc", commission_perc);

                int orderID = Convert.ToInt32(DbCommand.ExecuteScalar());//fetch the productID use it to rename image files
                //DatabaseRepository.writeToFile("db.txt", orderID+"");

                DbCon.Close();

                double agentProfit = 0;
                
               

                string product_report = "";
                int count = 0;

                using (MySqlConnection DbCon2 = new MySqlConnection(ConnectionString))
                {
                    DbCon2.Open();
                    Query = "INSERT INTO order_details (order_id,product_id,product_qty,product_price,product_agent_price_discount, product_agent_price_profit," +
                        "product_agent_discounted) VALUES(@orderID,@productID,@productQty,@productPrice,@productDiscount,@productAgentProfit,@productDiscounted)";

                    foreach(var product in ClientOrder.Products) {
                        using (var DataCommand = new MySqlCommand(Query, DbCon2))
                        {
                            DataCommand.Parameters.AddWithValue("@orderID", orderID);
                            DataCommand.Parameters.AddWithValue("@productID", product.id);
                            DataCommand.Parameters.AddWithValue("@productQty", product.Qty);
                            DataCommand.Parameters.AddWithValue("@productPrice", product.Price);
                            DataCommand.Parameters.AddWithValue("@productDiscount", product.Discount);
                            DataCommand.Parameters.AddWithValue("@productAgentProfit", product.Discount);
                            DataCommand.Parameters.AddWithValue("@productDiscounted", product.IsDiscounted);

                            total += double.Parse(product.Price, NumberStyles.Float, culture) * product.Qty;

                            if (product.IsDiscounted)
                            {
                                double price_ = (double.Parse(product.Price, NumberStyles.Float, culture) + product.Discount);
                                double unit_profit = price_ * commission_perc;// don't multiply by the quantity of items * product.Qty;
                                agentProfit += unit_profit;
                                var ProductData = GetProductById(product.id + "");
                                //product_report += "[" + (++count) + "] " + ProductData.Name + " - $" + price_ + " x " + product.Qty + " x " + commission_perc + ": $" + unit_profit+"\n";
                                //product_report += "[" + (++count) + "] " + ProductData.Name + " - $" + price_ + " x " + commission_perc + ": $" + unit_profit + "\n";
                                product_report += "[" + (++count) + "] " + ProductData.Name + " - $" + price_ + ": $" + unit_profit + "\n";
                            }
                            DataCommand.ExecuteNonQuery();
                        }
                    }
                    product_report += "\n*Total: $" + agentProfit + "*";
                    orderSuccess = true;
                    DbCon2.Close();
                }

                string orderType;
                //-- get order type
                if (ClientOrder.OrderType == (int)OrderType.CreditCardDelivery)//add his money straight to the bank
                {
                    orderType = "Carte Crédit à livrer";
                }
                else if (ClientOrder.OrderType == (int)OrderType.CreditCardCollection)//add his money straight to the bank
                {
                    orderType = "Carte Crédit à rétirer";
                }
                else if (ClientOrder.OrderType == (int)OrderType.ProductDelivery)//do nothing
                {
                    orderType = "Paiement à la livraison";
                }
                else if (ClientOrder.OrderType == (int)OrderType.ProductCollection)// do nothing
                {
                    orderType = "Paiement au retrait";
                }
                else
                {
                    orderType = "";
                }
                //--

                string href = DatabaseRepository.WebUrl + "Login/Admin";
                string messageOwner = $"<b>Rapport Juni</b><br/><br/>Une commande de {"<b>$" + total + "</b>"} vient d'etre effectué<br/><br/>Client: {"<b>" + ClientOrder.SenderCell + "</b>"}<br/>ID: {"<b>" + ClientOrder.OrderUniqueId + "</b>"}<br/>Type: {"<b>" + orderType + "</b>"}<br/><br/><a href='{href}'>Gérer la commande.</a><br/>";
                string[] emailList = GetNotificationEmails();
                string[] cellList = GetNotificationCells();
                string subject = "Juni - Notification de Commande";
                SendEmailInBackground(emailList, subject, messageOwner);

                string messageOwnerWApp = $"*Rapport Juni*\r\n\r\nUne commande de {"*$" + total + "*"} vient d'etre effectué\r\nClient: {"*" + ClientOrder.SenderCell + "*"}\r\nID: {"*" + ClientOrder.OrderUniqueId + "*"}\r\nType: {"*" + orderType + "*"}\r\n\r\n{DatabaseRepository.WebUrl}";
                for (int i = 0; i < cellList.Length; i++)
                {
                   // SendWhatsAppMessage(cellList[i], messageOwnerWApp);
                }

                //--->
                if (agentProfit > 0 && ClientOrder.IsDiscounted)//just making sure
                {
                    if (ClientOrder.OrderType == (int)OrderType.CreditCardDelivery)//add his money straight to the bank
                    {
                        User CurAgent = GetUserByUsername(ClientOrder.CouponCode);
                        //DatabaseRepository.writeToFile("agent.txt", CurAgent.coupon_code + "|" + CurAgent.id);
                        if (CurAgent != null)
                        {
                            UpdateAgentBalance(CurAgent.id + "", agentProfit);//update the agent money and let him know
                            double balance = GetAgentBalance(CurAgent.id+"");                            
                            /*string message = "*Rapport Juni*\n\n"+
                                "Agent: *"+CurAgent.phone_number+"*\n"
                                +"Vous avez obtenu un profit de $" + agentProfit + " sur la commande\n" +
                                "*" + ClientOrder.OrderUniqueId + "*\n*Carte Crédit à livrer*\n" +
                                "Client: *"+ClientOrder.SenderCell+ "*\nCommission: *"+(commission_perc*100)+"%*"+
                                "\n\n" +product_report+
                                "\n*Solde Agent: $" + balance + "*\n\nRassurez vous de la livraison du produit\n"+DatabaseRepository.WebUrl;
                            */
                            string message = $"*Rapport Juni*\r\n\r\nAgent: {"*"+CurAgent.phone_number+"*"}\r\nVous avez obtenu un profit de {"$"+agentProfit} sur\r\nla commande {"*"+ClientOrder.OrderUniqueId+"*"}\r\n{"*"+orderType+"*"}\r\nClient:{" *"+ ClientOrder.SenderCell+"*"} \r\nPour plus de détails, verifier votre inventaire.\r\nNouveau solde Agent: {"$"+balance}\r\n{DatabaseRepository.WebUrl}";                                                        
                            //SendWhatsAppMessage("+27722264804", message);
                        }
                    }
                    else if (ClientOrder.OrderType == (int)OrderType.CreditCardCollection)//add his money straight to the bank
                    {
                        User CurAgent = GetUserByUsername(ClientOrder.CouponCode);
                        if (CurAgent != null)
                        {
                            UpdateAgentBalance(CurAgent.id + "", agentProfit);//update the agent money and let him know
                            double balance = GetAgentBalance(CurAgent.id + "");                            
                            /*
                            string message = "*Rapport Juni*\n\nVous avez obtenu un profit de $" + agentProfit + " sur\n" +
                                "la commande *" + ClientOrder.OrderUniqueId + "*\nMot de Paiement: *Carte Crédit à rétirer*\n" +
                                "Client:*" + ClientOrder.SenderCell + "*\nCommission: *" + (commission_perc * 100) + "%*" +
                                "\n\n" + product_report +
                                "\n*Solde Agent: $" + balance + "*\n" + DatabaseRepository.WebUrl;*/
                            
                            string message = $"*Rapport Juni*\r\n\r\nAgent: {"*" + CurAgent.phone_number + "*"}\r\nVous avez obtenu un profit de {"$" + agentProfit} sur\r\nla commande {"*" + ClientOrder.OrderUniqueId + "*"}\r\n{"*" + orderType + "*"}\r\nClient:{" *" + ClientOrder.SenderCell + "*"} \r\nPour plus de détails, verifier votre inventaire.\r\nNouveau solde Agent: {"$" + balance}\r\n{DatabaseRepository.WebUrl}";                                                        
                            //SendWhatsAppMessage("+27722264804", message);
                        }

                    }
                    else if (ClientOrder.OrderType == (int)OrderType.ProductDelivery)//do nothing
                    {

                    }
                    else if (ClientOrder.OrderType == (int)OrderType.ProductCollection)// do nothing
                    {

                    }
                    else
                    {

                    }
                }
                

            }
            return orderSuccess;
        }
        
        public static void UpdateAgentBalance(string agent_id, double amount)
        {
            
            bool agentRegistered = IsAgentBankRegistered(agent_id);
            DatabaseRepository.writeToFile("agent.txt", agentRegistered+"");
            if (agentRegistered)
            {
                using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
                {
                    DbCon.Open();

                    string Query = "UPDATE agent_bank SET balance = balance + @newBalance WHERE agent_id=@agentId";
                    MySqlCommand DbCommand = new MySqlCommand(Query, DbCon);
                    DbCommand.Parameters.AddWithValue("@agentId", agent_id);
                    DbCommand.Parameters.AddWithValue("@newBalance", amount);

                    int productID = Convert.ToInt32(DbCommand.ExecuteScalar());//fetch the productID use it to rename image files                    
                    DbCon.Close();
                }
            }
            else
            {
                using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
                {
                    DbCon.Open();

                    string Query = "INSERT INTO agent_bank(agent_id,balance) VALUES(@agentId,@balance)";
                    MySqlCommand DbCommand = new MySqlCommand(Query, DbCon);
                    DbCommand.Parameters.AddWithValue("@agentId", agent_id);
                    DbCommand.Parameters.AddWithValue("@balance", amount);     

                    int productID = Convert.ToInt32(DbCommand.ExecuteScalar());//fetch the productID use it to rename image files                    
                    DbCon.Close();
                }

            }

           

        }

        public static double GetAgentBalance(string username)
        {
            double balance = 0;
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT balance FROM agent_bank WHERE (agent_id='" + username + "')", DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    balance = Convert.ToDouble(DbReader["balance"]);
                }
                DbCon.Close();
            }
            return balance;
        }

        public static bool IsAgentBankRegistered(string username)
        {
            bool flag = false;
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT agent_id FROM agent_bank WHERE (agent_id='" + username + "')", DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    flag = true;
                }
                DbCon.Close();
            }
            return flag;
        }

        public static User GetUserByUsername(string username)
        {
            User CurUser = null;
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM user_profile WHERE (email='"+username+"') OR " +
                    "(username='"+username+"') OR (phone_number='"+username+"') OR (coupon_code='"+username+"')", DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    CurUser = new User();
                    CurUser.id = Convert.ToInt32(DbReader["user_id"]);
                    CurUser.name = DbReader["name"] as string ?? CurUser.name;
                    CurUser.surname = DbReader["surname"] as string ?? CurUser.surname;
                    CurUser.coupon_code = DbReader["coupon_code"] as string ?? CurUser.coupon_code;
                    CurUser.phone_number = (string)DbReader["phone_number"];
                    CurUser.is_agent_approved = Convert.ToInt32(DbReader["agent_approved"])>0?true:false;
                    CurUser.username = DbReader["username"] as string ?? CurUser.username;
                    CurUser.email = DbReader["email"] as string ?? CurUser.email; 
                    CurUser.user_role_id = Convert.ToInt32(DbReader["user_role_id"]);
                }
                DbCon.Close();
            }
            return CurUser;
        }

        public static List<User> GetUserList()
        {
            List<User> UserList = new List<User>();
            
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM user_profile", DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    User CurUser = new User();
                    CurUser.id = Convert.ToInt32(DbReader["user_id"]);
                    CurUser.name = DbReader["name"] as string ?? CurUser.name;
                    CurUser.surname = DbReader["surname"] as string ?? CurUser.surname;
                    CurUser.phone_number = (string)DbReader["phone_number"];
                    CurUser.coupon_code = DbReader["coupon_code"] as string ?? CurUser.coupon_code;
                    CurUser.username = DbReader["username"] as string ?? CurUser.username;
                    CurUser.email = DbReader["email"] as string ?? CurUser.email;
                    CurUser.is_agent_approved = Convert.ToInt32(DbReader["agent_approved"]) > 0 ? true : false;
                    CurUser.user_role_id = Convert.ToInt32(DbReader["user_role_id"]);

                    UserList.Add(CurUser);
                }
                DbCon.Close();
            }
            return UserList;
        }

        public static List<User> GetUserListByType(int user_role_id)
        {
            List<User> UserList = new List<User>();

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand;
                if (user_role_id == (int)(UserRole.Agent))
                {
                     DbCommand = new MySqlCommand("SELECT * FROM user_profile WHERE (agent_approved=1) OR (user_role_id=" + user_role_id+")", DbCon);
                }
                else
                {
                     DbCommand = new MySqlCommand("SELECT * FROM user_profile WHERE user_role_id=" + user_role_id, DbCon);
                }
                

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    User CurUser = new User();
                    CurUser.id = Convert.ToInt32(DbReader["user_id"]);
                    CurUser.name = DbReader["name"] as string ?? CurUser.name;
                    CurUser.surname = DbReader["surname"] as string ?? CurUser.surname;
                    CurUser.phone_number = (string)DbReader["phone_number"];
                    CurUser.username = DbReader["username"] as string ?? CurUser.username;
                    CurUser.coupon_code = DbReader["coupon_code"] as string ?? CurUser.coupon_code;
                    CurUser.email = DbReader["email"] as string ?? CurUser.email;
                    CurUser.user_role_id = Convert.ToInt32(DbReader["user_role_id"]);
                    CurUser.is_agent_approved = Convert.ToInt32(DbReader["agent_approved"]) > 0 ? true : false;

                    UserList.Add(CurUser);
                }
                DbCon.Close();
            }
            return UserList;
        }

        public static Order GetOrderById(string orderID)
        {
            Order CurOrder = null;
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM order_table WHERE order_unique_id='"+orderID+"' OR id="+orderID, DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    CurOrder.OrderId = Convert.ToInt32(DbReader["id"]);
                    CurOrder.ClientId = Convert.ToInt32(DbReader["customer_id"]) + "";
                    CurOrder.OrderType = Convert.ToInt32(DbReader["order_type_id"]);
                    CurOrder.SenderFullname = DbReader["sender_fullname"] as string ?? CurOrder.SenderFullname;
                    CurOrder.SenderCell = DbReader["sender_cell"] as string ?? CurOrder.SenderCell;
                    CurOrder.Address = DbReader["dispatch_address"] as string ?? CurOrder.Address;
                    CurOrder.DestinatorFullname = DbReader["dest_fullname"] as string ?? CurOrder.DestinatorFullname;
                    CurOrder.DestinatorCell = DbReader["dest_cell"] as string ?? CurOrder.DestinatorCell;
                    CurOrder.GiftMessage = DbReader["dest_gift_message"] as string ?? CurOrder.GiftMessage;
                    CurOrder.OrderDate = DbReader["order_date"] as string ?? CurOrder.OrderDate;
                    CurOrder.DeliveryFee = Convert.ToDouble(DbReader["deliveryFee"]);
                    CurOrder.OrderUniqueId = (string)DbReader["order_unique_id"];
                    CurOrder.OrderCompleted = Convert.ToBoolean(DbReader["completed"]);

                    using (MySqlConnection DbCon2 = new MySqlConnection(ConnectionString))
                    {
                        DbCon2.Open();
                        MySqlCommand DbCommand2 = new MySqlCommand("SELECT * FROM order_details WHERE order_id=" + CurOrder.OrderId, DbCon2);
                        MySqlDataReader DbReader2 = DbCommand2.ExecuteReader();

                        List<Product> ProductList = new List<Product>();
                        while (DbReader2.Read())
                        {
                            Product CurProduct = new Product();
                            CurProduct.id = Convert.ToInt32(DbReader2["product_id"]);
                            CurProduct.Qty = Convert.ToInt32(DbReader2["product_qty"]);
                            CurProduct.Price = Convert.ToDouble(DbReader2["product_price"]).ToString();
                            ProductList.Add(CurProduct);
                        }
                        DbCon2.Close();

                        CurOrder.Products = ProductList;
                    }                                    

                }
                DbCon.Close();
            }
            return CurOrder;
        }

        public static List<Order> GetAllOrder()
        {
            List<Order> OrderList = new List<Order>();
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM order_table", DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    Order CurOrder = new Order();
                    CurOrder.OrderId = Convert.ToInt32(DbReader["id"]);
                    CurOrder.OrderType = Convert.ToInt32(DbReader["order_type_id"]);
                    CurOrder.ClientId = Convert.ToInt32(DbReader["customer_id"]) + "";
                    CurOrder.SenderFullname = DbReader["sender_fullname"] as string ?? CurOrder.SenderFullname;
                    CurOrder.SenderCell = DbReader["sender_cell"] as string ?? CurOrder.SenderCell;
                    CurOrder.Address = DbReader["dispatch_address"] as string ?? CurOrder.Address;
                    CurOrder.DestinatorFullname = DbReader["dest_fullname"] as string ?? CurOrder.DestinatorFullname;
                    CurOrder.DestinatorCell = DbReader["dest_cell"] as string ?? CurOrder.DestinatorCell;
                    CurOrder.GiftMessage = DbReader["dest_gift_message"] as string ?? CurOrder.GiftMessage;
                    CurOrder.OrderDate = DbReader["order_date"] as string ?? CurOrder.OrderDate;
                    CurOrder.DeliveryFee = Convert.ToDouble(DbReader["deliveryFee"]);
                    CurOrder.OrderUniqueId = (string)DbReader["order_unique_id"];
                    CurOrder.OrderCompleted = Convert.ToBoolean(DbReader["completed"]);

                    using (MySqlConnection DbCon2 = new MySqlConnection(ConnectionString))
                    {
                        DbCon2.Open();
                        MySqlCommand DbCommand2 = new MySqlCommand("SELECT * FROM order_details WHERE order_id="+CurOrder.OrderId, DbCon2);
                        MySqlDataReader DbReader2 = DbCommand2.ExecuteReader();

                        List<Product> ProductList = new List<Product>();
                        while (DbReader2.Read())
                        {
                            Product CurProduct = new Product();
                            CurProduct.id = Convert.ToInt32(DbReader2["product_id"]);
                            CurProduct.Qty = Convert.ToInt32(DbReader2["product_qty"]);
                            CurProduct.Price = Convert.ToDouble(DbReader2["product_price"]).ToString();
                            ProductList.Add(CurProduct);
                        }
                        DbCon2.Close();

                        CurOrder.Products = ProductList;
                    }

                    OrderList.Add(CurOrder);//Add current order

                }
                DbCon.Close();
            }
            return OrderList;
        }

        public static List<Order> GetAllOrderForDelivery()
        {
            List<Order> OrderList = new List<Order>();
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM order_table WHERE completed=0 AND (order_type_id=2 OR order_type_id=4)", DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    Order CurOrder = new Order();
                    CurOrder.OrderId = Convert.ToInt32(DbReader["id"]);
                    CurOrder.OrderType = Convert.ToInt32(DbReader["order_type_id"]);
                    CurOrder.ClientId = Convert.ToInt32(DbReader["customer_id"]) + "";
                    CurOrder.SenderFullname = DbReader["sender_fullname"] as string ?? CurOrder.SenderFullname;
                    CurOrder.SenderCell = DbReader["sender_cell"] as string ?? CurOrder.SenderCell;
                    CurOrder.Address = DbReader["dispatch_address"] as string ?? CurOrder.Address;
                    CurOrder.DestinatorFullname = DbReader["dest_fullname"] as string ?? CurOrder.DestinatorFullname;
                    CurOrder.DestinatorCell = DbReader["dest_cell"] as string ?? CurOrder.DestinatorCell;
                    CurOrder.GiftMessage = DbReader["dest_gift_message"] as string ?? CurOrder.GiftMessage;
                    CurOrder.OrderDate = DbReader["order_date"] as string ?? CurOrder.OrderDate;
                    CurOrder.DeliveryFee = Convert.ToDouble(DbReader["deliveryFee"]);
                    CurOrder.OrderUniqueId = (string)DbReader["order_unique_id"];
                    CurOrder.OrderCompleted = Convert.ToBoolean(DbReader["completed"]);

                    using (MySqlConnection DbCon2 = new MySqlConnection(ConnectionString))
                    {
                        DbCon2.Open();
                        MySqlCommand DbCommand2 = new MySqlCommand("SELECT * FROM order_details WHERE order_id=" + CurOrder.OrderId, DbCon2);
                        MySqlDataReader DbReader2 = DbCommand2.ExecuteReader();

                        List<Product> ProductList = new List<Product>();
                        while (DbReader2.Read())
                        {
                            Product CurProduct = new Product();
                            CurProduct.id = Convert.ToInt32(DbReader2["product_id"]);
                            CurProduct.Qty = Convert.ToInt32(DbReader2["product_qty"]);
                            CurProduct.Price = Convert.ToDouble(DbReader2["product_price"]).ToString();
                            ProductList.Add(CurProduct);
                        }
                        DbCon2.Close();

                        CurOrder.Products = ProductList;
                    }

                    OrderList.Add(CurOrder);//Add current order

                }
                DbCon.Close();
            }
            return OrderList;
        }

        public static List<Order> GetAllOrderForCollection()
        {
            List<Order> OrderList = new List<Order>();
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM order_table WHERE completed=0 AND (order_type_id=1 OR order_type_id=3)", DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    Order CurOrder = new Order();
                    CurOrder.OrderId = Convert.ToInt32(DbReader["id"]);
                    CurOrder.OrderType = Convert.ToInt32(DbReader["order_type_id"]);
                    CurOrder.ClientId = Convert.ToInt32(DbReader["customer_id"]) + "";
                    CurOrder.SenderFullname = DbReader["sender_fullname"] as string ?? CurOrder.SenderFullname;
                    CurOrder.SenderCell = DbReader["sender_cell"] as string ?? CurOrder.SenderCell;
                    CurOrder.Address = DbReader["dispatch_address"] as string ?? CurOrder.Address;
                    CurOrder.DestinatorFullname = DbReader["dest_fullname"] as string ?? CurOrder.DestinatorFullname;
                    CurOrder.DestinatorCell = DbReader["dest_cell"] as string ?? CurOrder.DestinatorCell;
                    CurOrder.GiftMessage = DbReader["dest_gift_message"] as string ?? CurOrder.GiftMessage;
                    CurOrder.OrderDate = DbReader["order_date"] as string ?? CurOrder.OrderDate;
                    CurOrder.DeliveryFee = Convert.ToDouble(DbReader["deliveryFee"]);
                    CurOrder.OrderUniqueId = (string)DbReader["order_unique_id"];
                    CurOrder.OrderCompleted = Convert.ToBoolean(DbReader["completed"]);

                    using (MySqlConnection DbCon2 = new MySqlConnection(ConnectionString))
                    {
                        DbCon2.Open();
                        MySqlCommand DbCommand2 = new MySqlCommand("SELECT * FROM order_details WHERE order_id=" + CurOrder.OrderId, DbCon2);
                        MySqlDataReader DbReader2 = DbCommand2.ExecuteReader();

                        List<Product> ProductList = new List<Product>();
                        while (DbReader2.Read())
                        {
                            Product CurProduct = new Product();
                            CurProduct.id = Convert.ToInt32(DbReader2["product_id"]);
                            CurProduct.Qty = Convert.ToInt32(DbReader2["product_qty"]);
                            CurProduct.Price = Convert.ToDouble(DbReader2["product_price"]).ToString();
                            ProductList.Add(CurProduct);
                        }
                        DbCon2.Close();

                        CurOrder.Products = ProductList;
                    }

                    OrderList.Add(CurOrder);//Add current order

                }
                DbCon.Close();
            }
            return OrderList;
        }

        public static List<Order> GetAllOrderIncomplete()
        {
            List<Order> OrderList = new List<Order>();
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM order_table WHERE completed=0", DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    Order CurOrder = new Order();
                    CurOrder.OrderId = Convert.ToInt32(DbReader["id"]);
                    CurOrder.OrderType = Convert.ToInt32(DbReader["order_type_id"]);
                    CurOrder.ClientId = Convert.ToInt32(DbReader["customer_id"]) + "";
                    CurOrder.SenderFullname = DbReader["sender_fullname"] as string ?? CurOrder.SenderFullname;
                    CurOrder.SenderCell = DbReader["sender_cell"] as string ?? CurOrder.SenderCell;
                    CurOrder.Address = DbReader["dispatch_address"] as string ?? CurOrder.Address;
                    CurOrder.DestinatorFullname = DbReader["dest_fullname"] as string ?? CurOrder.DestinatorFullname;
                    CurOrder.DestinatorCell = DbReader["dest_cell"] as string ?? CurOrder.DestinatorCell;
                    CurOrder.GiftMessage = DbReader["dest_gift_message"] as string ?? CurOrder.GiftMessage;
                    CurOrder.OrderDate = DbReader["order_date"] as string ?? CurOrder.OrderDate;
                    CurOrder.DeliveryFee = Convert.ToDouble(DbReader["deliveryFee"]);
                    CurOrder.OrderUniqueId = (string)DbReader["order_unique_id"];
                    CurOrder.OrderCompleted = Convert.ToBoolean(DbReader["completed"]);

                    using (MySqlConnection DbCon2 = new MySqlConnection(ConnectionString))
                    {
                        DbCon2.Open();
                        MySqlCommand DbCommand2 = new MySqlCommand("SELECT * FROM order_details WHERE order_id=" + CurOrder.OrderId, DbCon2);
                        MySqlDataReader DbReader2 = DbCommand2.ExecuteReader();

                        List<Product> ProductList = new List<Product>();
                        while (DbReader2.Read())
                        {
                            Product CurProduct = new Product();
                            CurProduct.id = Convert.ToInt32(DbReader2["product_id"]);
                            CurProduct.Qty = Convert.ToInt32(DbReader2["product_qty"]);
                            CurProduct.Price = Convert.ToDouble(DbReader2["product_price"]).ToString();
                            ProductList.Add(CurProduct);
                        }
                        DbCon2.Close();

                        CurOrder.Products = ProductList;
                    }

                    OrderList.Add(CurOrder);//Add current order

                }
                DbCon.Close();
            }
            return OrderList;
        }

        public static int AddUser(string tel, string email, string password)
        {
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();

                string Query = "INSERT INTO user_profile(phone_number,username,email,password) VALUES(@phone,@username,@email,@password); SELECT LAST_INSERT_ID()";
                MySqlCommand DbCommand = new MySqlCommand(Query, DbCon);
                DbCommand.Parameters.AddWithValue("@phone", tel);
                DbCommand.Parameters.AddWithValue("@username", tel);
                if(!email.Contains("@"))
                {
                    DbCommand.Parameters.AddWithValue("@email", null);
                }
                else
                {
                    DbCommand.Parameters.AddWithValue("@email", email);
                }
                
                DbCommand.Parameters.AddWithValue("@password", password);
                

                int userID = Convert.ToInt32(DbCommand.ExecuteScalar());//fetch the productID use it to rename image files                    
                DbCon.Close();

                return userID;
            }

        }

        public static List<Order> GetAllSoldOrder()
        {
            List<Order> OrderList = new List<Order>();
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM order_table WHERE completed=1 OR (order_type_id=1 OR order_type_id=2)", DbCon);
                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    Order CurOrder = new Order();
                    CurOrder.OrderId = Convert.ToInt32(DbReader["id"]);
                    CurOrder.OrderType = Convert.ToInt32(DbReader["order_type_id"]);
                    CurOrder.ClientId = Convert.ToInt32(DbReader["customer_id"]) + "";
                    CurOrder.SenderFullname = DbReader["sender_fullname"] as string ?? CurOrder.SenderFullname;
                    CurOrder.SenderCell = DbReader["sender_cell"] as string ?? CurOrder.SenderCell;
                    CurOrder.Address = DbReader["dispatch_address"] as string ?? CurOrder.Address;
                    CurOrder.DestinatorFullname = DbReader["dest_fullname"] as string ?? CurOrder.DestinatorFullname;
                    CurOrder.DestinatorCell = DbReader["dest_cell"] as string ?? CurOrder.DestinatorCell;
                    CurOrder.GiftMessage = DbReader["dest_gift_message"] as string ?? CurOrder.GiftMessage;
                    CurOrder.OrderDate = DbReader["order_date"] as string ?? CurOrder.OrderDate;
                    CurOrder.DeliveryFee = Convert.ToDouble(DbReader["deliveryFee"]);
                    CurOrder.OrderUniqueId = (string)DbReader["order_unique_id"];
                    CurOrder.OrderCompleted = Convert.ToBoolean(DbReader["completed"]);

                    using (MySqlConnection DbCon2 = new MySqlConnection(ConnectionString))
                    {
                        DbCon2.Open();
                        MySqlCommand DbCommand2 = new MySqlCommand("SELECT * FROM order_details WHERE order_id=" + CurOrder.OrderId, DbCon2);
                        MySqlDataReader DbReader2 = DbCommand2.ExecuteReader();

                        List<Product> ProductList = new List<Product>();
                        while (DbReader2.Read())
                        {
                            Product CurProduct = new Product();
                            CurProduct.id = Convert.ToInt32(DbReader2["product_id"]);
                            CurProduct.Qty = Convert.ToInt32(DbReader2["product_qty"]);
                            CurProduct.Price = Convert.ToDouble(DbReader2["product_price"]).ToString();
                            ProductList.Add(CurProduct);
                        }
                        DbCon2.Close();

                        CurOrder.Products = ProductList;
                    }

                    OrderList.Add(CurOrder);//Add current order

                }
                DbCon.Close();
            }
            return OrderList;
        }

        public static double GetTotalRevenue()
        {
            List<Order> OrderList = GetAllSoldOrder();
            double total = 0;
            foreach(Order CurOrder in OrderList){
               total+= Order.GetOrderTotal(CurOrder);
            }
            return total;
        }
        #region
        
        public static int GetTotalForDelivery()
        {
            int count = 0;

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT COUNT(*) AS 'deliveryCount' FROM order_table WHERE completed=0 AND (order_type_id=2 OR order_type_id=4)", DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    count = Convert.ToInt32(DbReader["deliveryCount"]);
                }
                DbCon.Close();
            }
            return count;
        }

        public static int GetTotalForCollection()
        {
            int count = 0;

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT COUNT(*) AS 'collectionCount' FROM order_table WHERE completed=0 AND (order_type_id=1 OR order_type_id=3)", DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    count = Convert.ToInt32(DbReader["collectionCount"]);
                }
                DbCon.Close();
            }
            return count;
        }

        public static int GetTotalCompleted()
        {
            int count = 0;

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT COUNT(*) AS 'totalCount' FROM order_table WHERE completed=1", DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    count = Convert.ToInt32(DbReader["totalCount"]);
                }
                DbCon.Close();
            }
            return count;
        }

        public static int GetVisitCount()
        {
            int count = 0;

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT value FROM configuration WHERE key_name='num_visits'", DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    count = Convert.ToInt32(DbReader["value"]);
                }
                DbCon.Close();
            }
            return count;
        }

        public static int GetClientCount()
        {
            int count = 0;

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT COUNT(*) AS 'total_clients' FROM user_profile", DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    count = Convert.ToInt32(DbReader["total_clients"]);
                }
                DbCon.Close();
            }
            return count;
        }

        public static int GetSalesCount()
        {
            int count = 0;

            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT COUNT(*) AS 'total_sales' FROM order_table WHERE completed=1 OR order_type_id=1 OR order_type_id=2", DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    count = Convert.ToInt32(DbReader["total_sales"]);
                }
                DbCon.Close();
            }
            return count;
        }

        #endregion



        #region  
        //application
        public static bool AddApplication(AgentApplication CurApplication)
        {

            bool orderSuccess = false;
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();

                string Query = "INSERT INTO agent_application(tel,full_name,email,street_address,suburb,municipality,city,province,application_date,application_approved)" +
                    " VALUES(@tel,@fullName,@email,@streetAddress,@suburb,@municipality,@city,@province,@applicationDate,@applicationStatus); SELECT LAST_INSERT_ID()";

                MySqlCommand DbCommand = new MySqlCommand(Query, DbCon);
                DbCommand.Parameters.AddWithValue("@tel", CurApplication.CellNumber);
                DbCommand.Parameters.AddWithValue("@fullName", CurApplication.Name);
                DbCommand.Parameters.AddWithValue("@email", CurApplication.Email);
                DbCommand.Parameters.AddWithValue("@streetAddress", CurApplication.StreetAddress);
                DbCommand.Parameters.AddWithValue("@suburb", CurApplication.Suburb);
                DbCommand.Parameters.AddWithValue("@municipality", CurApplication.Municipality);
                DbCommand.Parameters.AddWithValue("@city", CurApplication.City);
                DbCommand.Parameters.AddWithValue("@province", CurApplication.Province);
                CurApplication.Date = DatabaseRepository.DateNow();//get current date
                DbCommand.Parameters.AddWithValue("@applicationDate", CurApplication.Date);
                DbCommand.Parameters.AddWithValue("@applicationStatus", CurApplication.IsApproved);
               

                int applicationID = Convert.ToInt32(DbCommand.ExecuteScalar());//fetch the productID use it to rename image files
                DatabaseRepository.writeToFile("db.txt", applicationID + "");

                DbCon.Close();

                //save the file
                int id = CurApplication.IdFileName.IndexOf(".");
                string newName = "identité_"+applicationID + "_" + CurApplication.CellNumber +CurApplication.IdFileName.Substring(id);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "application", newName);
                File.WriteAllBytes(path, CurApplication.IdFileContent);

                string rel_filename = "img/application/" + newName;

                using (MySqlConnection DbCon2 = new MySqlConnection(ConnectionString))
                {
                    DbCon2.Open();
                    Query = "INSERT INTO application_docs(path,app_id) VALUES(@path,@app_id)";
                    MySqlCommand DbCommand2 = new MySqlCommand(Query, DbCon2);
                    DbCommand2.Parameters.AddWithValue("@path", rel_filename);
                    DbCommand2.Parameters.AddWithValue("@app_id", applicationID);
                    DbCommand2.ExecuteNonQuery();
                    DbCon2.Close();
                }

                orderSuccess = true;
            }
            return orderSuccess;
        }

        public static int ApproveApplication(string application_id)
        {
            int success = 0;
            AgentApplication CurApplication = DatabaseRepository.GetAgentApplicationById(application_id);           
            using (MySqlConnection DbCon2 = new MySqlConnection(ConnectionString))
            {
                DbCon2.Open();
                MySqlCommand DbCommand2 = new MySqlCommand("SELECT * FROM user_profile WHERE phone_number='" + CurApplication.CellNumber + "'", DbCon2);
                MySqlDataReader DbReader2 = DbCommand2.ExecuteReader();
                using (MySqlConnection DbCon3 = new MySqlConnection(ConnectionString))
                {
                    DbCon3.Open();
                    string Query;
                    string CouponCode = GenerateCouponCode(CurApplication.id);

                    //if it is an existing user
                    if (DbReader2.Read())
                    {
                        //approve agent status
                        Query = "UPDATE user_profile SET agent_approved=1,coupon_code='"+CouponCode+"' WHERE phone_number='"+ CurApplication.CellNumber+ "'";
                        MySqlCommand DbCommand3 = new MySqlCommand(Query, DbCon3);
                        DbCommand3.ExecuteNonQuery();

                        success = 1;//existing customer
                    }
                    else//create user and grant access
                    {
                        Query = "INSERT INTO user_profile (phone_number,password,username,email," +
                            "user_role_id,agent_approved,coupon_code) VALUES(@phone,@password,@username,@email,@userRole,@agentApproved,@coupon_code)";
                        MySqlCommand DbCommand3 = new MySqlCommand(Query, DbCon3);
                        DbCommand3.Parameters.AddWithValue("@phone",CurApplication.CellNumber);
                        DbCommand3.Parameters.AddWithValue("@email", CurApplication.Email);
                        DbCommand3.Parameters.AddWithValue("@username", CurApplication.CellNumber);
                        DbCommand3.Parameters.AddWithValue("@userRole", (int)UserRole.Agent);
                        DbCommand3.Parameters.AddWithValue("@agentApproved", 1);
                        DbCommand3.Parameters.AddWithValue("@password","12345");
                        DbCommand3.Parameters.AddWithValue("@coupon_code", CouponCode);
                        DbCommand3.ExecuteNonQuery();

                        success = 2;//new customer
                    }

                    //approve application
                    Query = "UPDATE agent_application SET application_approved=1 WHERE id=" + CurApplication.id;
                    MySqlCommand DbCommand4 = new MySqlCommand(Query, DbCon3);
                    DbCommand4.ExecuteNonQuery();

                    DbCon3.Close();
                }                        
                DbCon2.Close();
            }

            return success;

        }

        #endregion

        public static String DateNow()
        {
            DateTime now = DateTime.Now;
            string formatDate = now.ToString("yyyy-MM-dd");
            return formatDate;
        }


        public static CouponProfile GetCouponProfileById(string couponCode)
        {
            CouponProfile CurCouponProfile = null;
            User CurUser = null;
            MySqlConnection DbCon;
            MySqlCommand DbCommand;
            MySqlDataReader DbReader;

            using (DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                DbCommand = new MySqlCommand("SELECT * FROM user_profile WHERE coupon_code='" + couponCode + "'", DbCon);
                DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    CurUser = new User();
                    CurUser.id = Convert.ToInt32(DbReader["user_id"]);
                    CurUser.name = DbReader["name"] as string ?? CurUser.name;
                    CurUser.surname = DbReader["surname"] as string ?? CurUser.surname;
                    CurUser.coupon_code = DbReader["coupon_code"] as string ?? CurUser.coupon_code;
                    CurUser.phone_number = (string)DbReader["phone_number"];
                    CurUser.is_agent_approved = Convert.ToInt32(DbReader["agent_approved"]) > 0 ? true : false;
                    CurUser.username = DbReader["username"] as string ?? CurUser.username;
                    CurUser.email = DbReader["email"] as string ?? CurUser.email;
                    CurUser.user_role_id = Convert.ToInt32(DbReader["user_role_id"]);
                }
                DbCon.Close();
            }

            if(CurUser != null)
            {
                CurCouponProfile = new CouponProfile();
                CurCouponProfile.Agent = CurUser;//store Agent details
            }

            
            using (DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                string query = "SELECT product.* FROM agent_market JOIN product ON agent_market.product_id = product.product_id JOIN user_profile " +
                    "ON user_profile.user_id = agent_market.agent_id WHERE user_profile.coupon_code='" + couponCode + "'";
                DbCommand = new MySqlCommand(query, DbCon);
                DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    if (CurCouponProfile.ProductList == null)
                        CurCouponProfile.ProductList = new List<Product>();

                    Product CurProduct = new Product();
                    CurProduct.id = Convert.ToInt32(DbReader["product_id"]);
                    CurProduct.Name = (string)DbReader["name"];
                    CurProduct.Description = (string)DbReader["description"];
                    CurProduct.Price = "" + DbReader["price"];
                    CurProduct.Qty = Convert.ToInt32(DbReader["quantity"]);
                    CurProduct.CategoryId = Convert.ToInt32(DbReader["category_id"]);
                    CurProduct.PreviewImagePaths = GetProductImagePaths(CurProduct.id);//Get Product Image Paths
                    CurCouponProfile.ProductList.Add(CurProduct);

                    //DatabaseRepository.writeToFile("agent_market.txt", CurProduct.Name+ " "+query);
                }
                DbCon.Close();

               
            }
            /*
            if(ProductList.Count > 0 && CurCouponProfile != null)
            {
                CurCouponProfile.ProductList = ProductList;//add product list
            }*/
            if(CurCouponProfile != null)
            {
                CurCouponProfile.Id = couponCode;
            }
            return CurCouponProfile;//return current coupon profile
        }



        public static CouponProfile GetAgentMarketById(string username)
        {
            CouponProfile CurCouponProfile = null;
            User CurUser = null;
            MySqlConnection DbCon;
            MySqlCommand DbCommand;
            MySqlDataReader DbReader;

            string query;

            using (DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                query = "SELECT * FROM user_profile WHERE (coupon_code='" + username + "')  OR (phone_number='" + username + "') OR (email='" + username + "') OR (user_id=" + username + ")";
                DatabaseRepository.writeToFile("agent_market1.txt", query);
                DbCommand = new MySqlCommand(query, DbCon);
                DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    CurUser = new User();
                    CurUser.id = Convert.ToInt32(DbReader["user_id"]);
                    CurUser.name = DbReader["name"] as string ?? CurUser.name;
                    CurUser.surname = DbReader["surname"] as string ?? CurUser.surname;
                    CurUser.coupon_code = DbReader["coupon_code"] as string ?? CurUser.coupon_code;
                    CurUser.phone_number = (string)DbReader["phone_number"];
                    CurUser.is_agent_approved = Convert.ToInt32(DbReader["agent_approved"]) > 0 ? true : false;
                    CurUser.username = DbReader["username"] as string ?? CurUser.username;
                    CurUser.email = DbReader["email"] as string ?? CurUser.email;
                    CurUser.user_role_id = Convert.ToInt32(DbReader["user_role_id"]);
                }
                DbCon.Close();
            }

            if (CurUser != null)
            {
                CurCouponProfile = new CouponProfile();
                CurCouponProfile.Agent = CurUser;//store Agent details
            }


            using (DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                query = "SELECT product.* FROM agent_market JOIN product ON agent_market.product_id = product.product_id JOIN user_profile " +
                    "ON user_profile.user_id = agent_market.agent_id WHERE  (user_profile.coupon_code='" + username + "')  OR (user_profile.phone_number='" + username + "') OR (user_profile.email='" + username + "') OR (user_profile.user_id=" + username + ")";

                DatabaseRepository.writeToFile("agent_market2.txt", query);

                DbCommand = new MySqlCommand(query, DbCon);
                DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    if (CurCouponProfile.ProductList == null)
                        CurCouponProfile.ProductList = new List<Product>();

                    Product CurProduct = new Product();
                    CurProduct.id = Convert.ToInt32(DbReader["product_id"]);
                    CurProduct.Name = (string)DbReader["name"];
                    CurProduct.Description = (string)DbReader["description"];
                    CurProduct.Price = "" + DbReader["price"];
                    CurProduct.Qty = Convert.ToInt32(DbReader["quantity"]);
                    CurProduct.CategoryId = Convert.ToInt32(DbReader["category_id"]);
                    CurProduct.PreviewImagePaths = GetProductImagePaths(CurProduct.id);//Get Product Image Paths
                    CurCouponProfile.ProductList.Add(CurProduct);

                   // DatabaseRepository.writeToFile("agent_market.txt", CurProduct.Name + " " + query);
                }
                DbCon.Close();


            }
            /*
            if(ProductList.Count > 0 && CurCouponProfile != null)
            {
                CurCouponProfile.ProductList = ProductList;//add product list
            }*/
            if (CurCouponProfile != null)
            {
                CurCouponProfile.Id = CurUser.coupon_code;
            }
            return CurCouponProfile;//return current coupon profile
        }


        public static int AddProductToAgentMarket(string product_id, string agent_id)
        {
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();

                string Query = "INSERT INTO agent_market(product_id,agent_id) VALUES(@productId,@agentId);";
                MySqlCommand DbCommand = new MySqlCommand(Query, DbCon);
                DbCommand.Parameters.AddWithValue("@productId", product_id);
                DbCommand.Parameters.AddWithValue("@agentId", agent_id);

                int num_rows_affected = Convert.ToInt32(DbCommand.ExecuteScalar());//fetch the productID use it to rename image files                    
                DbCon.Close();
                return num_rows_affected;
            }

        }

        public static int RemoveProductFromAgentMarket(string product_id, string agent_id)
        {
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();

                string Query = "DELETE FROM agent_market WHERE (product_id=" + product_id + ") AND (agent_id=" + agent_id+")";
                MySqlCommand DbCommand = new MySqlCommand(Query, DbCon);          

                int num_rows_affected = Convert.ToInt32(DbCommand.ExecuteScalar());//fetch the productID use it to rename image files                    
                DbCon.Close();
                return num_rows_affected;
            }

        }

        static string GenerateCouponCode(int uniqueId)
        {
            // Combine the unique ID with a random string
            string randomString = GenerateRandomString();
            string combinedString = uniqueId.ToString() + randomString;

            // Use a hash function to ensure a fixed length
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(combinedString));
                string hashedString = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 6);

                return hashedString;
            }
        }

        static string GenerateRandomString()
        {
            const string characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            Random random = new Random();

            // Generate a random string of length 6
            char[] randomString = new char[6];
            for (int i = 0; i < randomString.Length; i++)
            {
                randomString[i] = characters[random.Next(characters.Length)];
            }

            return new string(randomString);
        }

    }
}
