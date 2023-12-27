using Juni_Web_App.Models.Db;
using Newtonsoft.Json;

namespace Juni_Web_App.Models
{
    public class Order
    {
        //Ids
        public int OrderId { get; set; }
        public string OrderUniqueId { get; set; }
        
        //OrderType
        public int OrderType { get; set; }

        public string OrderDate { get; set; }

        public bool OrderCompleted { get; set; }

        public double DeliveryFee { get; set; }
                
        public List<Product> Products { get; set; }//list of products

        //--sender & destinator info
        public string Address { get; set; }//destination address
        public string SenderCell { get; set; }
        public string SenderFullname { get; set; }
        public string ClientId { get; set; }

        //Destinator
        public string DestinatorFullname { get; set; }
        public string DestinatorCell { get; set; }
        public string GiftMessage { get; set; }

        public string GetJSonProductNames()
        {
            List<string> names = new List<string>();
            for(int i=0; i < Products.Count; i++)
            {
                Product CurProduct = DatabaseRepository.GetProductById(Products[i].id+"");
                names.Add(CurProduct.Name);
            }
            return JsonConvert.SerializeObject(names);
        }

        public string GetJSonProductIds()
        {
            List<string> names = new List<string>();
            for (int i = 0; i < Products.Count; i++)
            {
                names.Add(Products[i].id + "");
            }
            return JsonConvert.SerializeObject(names);
        }

        public string GetJSonProductPrices()
        {
            List<string> prices = new List<string>();
            for (int i = 0; i < Products.Count; i++)
            {
                prices.Add(Products[i].Price);
            }
            return JsonConvert.SerializeObject(prices);
        }

        public string GetJSonProductQties()
        {
            List<string> qties = new List<string>();
            for (int i = 0; i < Products.Count; i++)
            {
                qties.Add(Products[i].Qty+"");
            }
            return JsonConvert.SerializeObject(qties);
        }

        public static double GetOrderTotal(Order order)
        {
            double deliveryFee = order.DeliveryFee;
            double total = 0;
            for(int i=0; i < order.Products.Count; i++)
            {
                total += order.Products[i].Qty * Convert.ToDouble(order.Products[i].Price);
            }
            total += deliveryFee;
            return total;
        }

        public static string OrderTypeMessage(int type)
        {
            switch (type)
            {
                case 1:
                    return "Carte-Crédit et Retrait";
                case 2:
                    return "Carte-Crédit et Livraison";
                case 3:
                    return "Retrait sur Place";
                case 4:
                    return "Paie à la Livraison";
                default:
                    return "Inconnu";
            }
        }
    }
}
