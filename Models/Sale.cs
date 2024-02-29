using Microsoft.Extensions.Primitives;

namespace Juni_Web_App.Models
{
    public class Sale
    {
        public string CouponCode { get; set; }
        public string AgentId { get; set; }
        public string OrderUniqueId { get; set; }
        public string ClientCell { get; set; }
        public string Date { get; set; }
        public bool IsDiscounted { get; set; }
        public bool IsCompleted { get; set; }
        public List<Product> ProductList { get; set; }
        public double Profit { get; set; }
        public double NetTotal { get; set; }
        public double Total { get; set; }
        public double DeliveryFee { get; set; }
        public double CommissionPerc { get; set; }
        public int OrderId { get; set; }
    }
}
