namespace Juni_Web_App.Models
{
    public class User
    {
        public int id { get; set; }
        public string name { get; set; }
        public string surname { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public int user_role_id { get; set; }
        public string phone_number { get; set; }
        public string coupon_code { get; set; }
        public bool is_agent_approved { get; set; }
        public static string UserTypeMessage(int type)
        {
            switch (type)
            {
                case 1:
                    return "Admin";
                case 2:
                    return "Client";
                case 3:
                    return "Agent";           
                default:
                    return "Inconnu";
            }
        }
    }
}
