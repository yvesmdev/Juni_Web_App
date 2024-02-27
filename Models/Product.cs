using Newtonsoft.Json;

namespace Juni_Web_App.Models
{
    public class Product
    {
        public Product()
        {
        }

        public Product(int id, string name, string price)
        {
            this.id = id;
            this.Name = name;
            this.Price = price;
        }
        public Product(int id, string name, string price, string desc)
        {
            this.id = id;
            this.Name = name;
            this.Price = price;
            this.Description = desc;
        }

        public Product(int id, string name, string price, string desc, int catid)
        {
            this.id = id;
            this.Name = name;
            this.Price = price;
            this.Description = desc;
            this.CategoryId = catid;
        }

        public string GetSerialisedImages()
        {
            return JsonConvert.SerializeObject(this.PreviewImagePaths);
        }

        public int id { get; set; }
        public string Name { get; set; }
        public string Price { get;set; }
        public string Description { get; set; }
        public double Discount { get; set; }
        public bool IsDiscounted { get; set; }
        public int Qty { get; set; }
        public int CategoryId { get; set; }
        public List<string> PreviewImagePaths { get; set; }
    }
}
