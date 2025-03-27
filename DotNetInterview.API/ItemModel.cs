namespace DotNetInterview.API.ItemModel
{
    public class Item
    {
        public int Id { get; set; } // Primary key
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
    }
}