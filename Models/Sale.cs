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

        public List<Product> ProductList { get; set; }

        public double profit { get; set; }
        public double netTotal { get; set; }
        public double total { get; set; }
        public double deliveryFee { get; set; }
        public double commissionPerc { get; set; }
        public int OrderId { get; set; }
    }
}
