using Website_Coffee.Models;

namespace Website_Coffee.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Product> Products { get; set; } = new List<Product>();
        public IEnumerable<Category> Categories { get; set; } = new List<Category>();
        public string? SearchString { get; set; }
        public int? CategoryId { get; set; }
    }
}
