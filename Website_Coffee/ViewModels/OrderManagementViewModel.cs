using Website_Coffee.Models;

namespace Website_Coffee.ViewModels
{
    public class OrderManagementViewModel
    {
        public IEnumerable<Order> Orders { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrders { get; set; }
    }
}
