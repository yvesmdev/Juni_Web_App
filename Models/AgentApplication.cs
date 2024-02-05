namespace Juni_Web_App.Models
{
    public class AgentApplication
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string CellNumber { get; set; }
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
    }
}
