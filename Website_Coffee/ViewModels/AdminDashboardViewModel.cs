using Website_Coffee.Models;

namespace Website_Coffee.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public IEnumerable<Order> RecentOrders { get; set; }
    }
}
