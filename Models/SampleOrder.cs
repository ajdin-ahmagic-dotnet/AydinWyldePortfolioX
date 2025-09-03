namespace AydinWyldePortfolioX.Models
{
    public class SampleOrder
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ShipCountry { get; set; }
        public string ShipCity { get; set; }


        // Make a sample private and public methods

        private static void PrivateMethod()
        {
            Console.WriteLine("This is a private method in SampleOrder class.");
        }

        public static void PublicMethod()
        {
            Console.WriteLine("This is a public method in SampleOrder class.");
        }
    }
}