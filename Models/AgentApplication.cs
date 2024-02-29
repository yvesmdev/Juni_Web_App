using Twilio.Types;

namespace Juni_Web_App.Models
{
    public class AgentApplication
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string CellNumber { get; set; }
        public string CountryCode { get; set; }
        public string Email { get; set; }
        public string StreetAddress { get; set; }
        public string Suburb { get; set; }
        public string Municipality { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string IdFileName { get; set; }
        public byte[] IdFileContent { get; set; }
        public bool IsApproved { get; set; }
        public string Date { get; set; }

        public string GetCountryNumber()
        {
            if (CellNumber[0] == '0')
            {
                return CountryCode + CellNumber.Substring(1);
            }
            else
            {
                return CountryCode + CellNumber;//.Substring(1);
            }
        }
    }
}
