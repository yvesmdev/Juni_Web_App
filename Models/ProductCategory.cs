namespace Juni_Web_App.Models
{
    public class ProductCategory
    {
        public ProductCategory()
        {
        }
        public ProductCategory(string id, string name)
        {
            Id = id;
            Name = name;
        }
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
