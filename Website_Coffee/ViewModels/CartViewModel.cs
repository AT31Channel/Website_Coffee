using Website_Coffee.Models;

namespace Website_Coffee.ViewModels
{
    public class CartViewModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public decimal Total { get; set; }
    }
}
