using Website_Coffee.Models;

namespace Website_Coffee.ViewModels
{
    public class CustomerOrdersViewModel
    {
        public List<Order> Orders { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
    }
}
