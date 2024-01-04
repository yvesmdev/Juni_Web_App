using Juni_Web_App.Models.Mobile;
using MySql.Data.MySqlClient;

namespace Juni_Web_App.Models.Db
{
    public class DatabaseRepository
    {
        public static string ConnectionString = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["DefaultConnection"];


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
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();

                string Query = "INSERT INTO order_table(customer_id,sender_fullname,sender_cell,dispatch_address,dest_fullname,dest_cell,dest_gift_message,order_date,order_type_id,deliveryFee,order_unique_id,completed)" +
                    " VALUES(@customerID,@senderFullname,@senderCell,@dispatchAddress,@destFullname,@destCell,@destGiftMessage,@orderDate,@orderTypeId,@deliveryFee,@orderUniqueId,@completed); SELECT LAST_INSERT_ID()";

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

                int orderID = Convert.ToInt32(DbCommand.ExecuteScalar());//fetch the productID use it to rename image files
                DatabaseRepository.writeToFile("db.txt", orderID+"");

                DbCon.Close();

                using (MySqlConnection DbCon2 = new MySqlConnection(ConnectionString))
                {
                    DbCon2.Open();
                    Query = "INSERT INTO order_details (order_id,product_id,product_qty,product_price) VALUES(@orderID,@productID,@productQty,@productPrice)";

                    foreach(var product in ClientOrder.Products) {
                        using (var DataCommand = new MySqlCommand(Query, DbCon2))
                        {
                            DataCommand.Parameters.AddWithValue("@orderID", orderID);
                            DataCommand.Parameters.AddWithValue("@productID", product.id);
                            DataCommand.Parameters.AddWithValue("@productQty", product.Qty);
                            DataCommand.Parameters.AddWithValue("@productPrice", product.Price);
                            DataCommand.ExecuteNonQuery();
                        }
                    }
                    orderSuccess = true;
                    DbCon2.Close();
                }
                

            }
            return orderSuccess;
        }

        public static User GetUserByUsername(string username)
        {
            User CurUser = null;
            using (MySqlConnection DbCon = new MySqlConnection(ConnectionString))
            {
                DbCon.Open();
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM user_profile WHERE (email='"+username+"') OR " +
                    "(username='"+username+"') OR (phone_number='"+username+"')", DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                if (DbReader.Read())
                {
                    CurUser = new User();
                    CurUser.id = Convert.ToInt32(DbReader["user_id"]);
                    CurUser.name = DbReader["name"] as string ?? CurUser.name;
                    CurUser.surname = DbReader["surname"] as string ?? CurUser.surname;
                    CurUser.phone_number = (string)DbReader["phone_number"];                    
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
                    CurUser.username = DbReader["username"] as string ?? CurUser.username;
                    CurUser.email = DbReader["email"] as string ?? CurUser.email;
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
                MySqlCommand DbCommand = new MySqlCommand("SELECT * FROM user_profile WHERE user_role_id="+user_role_id, DbCon);

                MySqlDataReader DbReader = DbCommand.ExecuteReader();
                while (DbReader.Read())
                {
                    User CurUser = new User();
                    CurUser.id = Convert.ToInt32(DbReader["user_id"]);
                    CurUser.name = DbReader["name"] as string ?? CurUser.name;
                    CurUser.surname = DbReader["surname"] as string ?? CurUser.surname;
                    CurUser.phone_number = (string)DbReader["phone_number"];
                    CurUser.username = DbReader["username"] as string ?? CurUser.username;
                    CurUser.email = DbReader["email"] as string ?? CurUser.email;
                    CurUser.user_role_id = Convert.ToInt32(DbReader["user_role_id"]);

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
        /*
        public static Product GetProductById(int id)
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
                    CurProduct.Name = (string)DbReader["name"];
                    CurProduct.Description = (string)DbReader["description"];
                    CurProduct.Price = Convert.ToDouble(DbReader["price"])+"";
                    CurProduct.Qty = Convert.ToInt32(DbReader["quantity"]);
                    CurProduct.CategoryId = Convert.ToInt32(DbReader["category_id"]);                    
                }
                DbCon.Close();
            }
            return CurProduct;
        }*/
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
    }
}
