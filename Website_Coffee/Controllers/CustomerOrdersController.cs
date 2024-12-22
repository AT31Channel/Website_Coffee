using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_Coffee.Data;
using Website_Coffee.Models;

namespace Website_Coffee.Controllers
{
    [Authorize]
    public class CustomerOrdersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerOrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: CustomerOrders
        public async Task<IActionResult> Index()
        {
            var userId = User.Identity.Name;
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Giữ lại thông báo thành công từ action Checkout
            if (TempData["Success"] != null)
            {
                ViewBag.SuccessMessage = TempData["Success"].ToString();
            }

            return View(orders);
        }

        // GET: CustomerOrders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.Identity.Name;
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.Identity.Name;
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == OrderStatus.Pending || order.Status == OrderStatus.Confirmed)
            {
                order.Status = OrderStatus.Cancelled;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đơn hàng đã được hủy thành công.";
            }
            else
            {
                TempData["Error"] = "Chỉ có thể hủy đơn hàng khi chưa được giao.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
