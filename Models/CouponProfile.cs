namespace Juni_Web_App.Models
{
    public class CouponProfile
    {
        public string Id { get; set; }
        public User Agent { get; set; }
        public List<Product> ProductList { get; set; }
        public CouponProfile(){
        }

    }
}
