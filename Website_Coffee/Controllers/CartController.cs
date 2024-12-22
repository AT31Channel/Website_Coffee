using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Website_Coffee.Data;
using Website_Coffee.Models;
using Website_Coffee.Services;
using Website_Coffee.ViewModels;

namespace Website_Coffee.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public CartController(ApplicationDbContext context, NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        private async Task<Cart> GetOrCreateCartAsync()
        {
            // Get the actual user ID instead of username
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new InvalidOperationException("User is not authenticated");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var cart = await GetOrCreateCartAsync();
            var viewModel = new CartViewModel
            {
                CartItems = cart.CartItems.ToList(),
                Total = cart.CartItems.Sum(item => item.Quantity * item.Product.Price)
            };

            return View(viewModel);
        }

        // POST: Cart/Add
        [HttpPost]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để thêm vào giỏ hàng" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { success = false, message = "Không thể xác định người dùng" });
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sản phẩm" });
            }

            var cart = await GetOrCreateCartAsync();
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }
            else
            {
                cartItem.Quantity += quantity;
                _context.CartItems.Update(cartItem);
            }

            await _context.SaveChangesAsync();

            // Lấy tổng số sản phẩm trong giỏ hàng
            var totalItems = await _context.CartItems
                .Where(c => c.Cart.UserId == userId)
                .SumAsync(c => c.Quantity);

            return Json(new { 
                success = true, 
                message = $"Đã thêm {quantity} {product.Name} vào giỏ hàng",
                totalItems = totalItems
            });
        }

        // POST: Cart/Update
        [HttpPost]
        public async Task<IActionResult> Update(int productId, int quantity)
        {
            var cart = await GetOrCreateCartAsync();
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem == null)
            {
                return NotFound();
            }

            if (quantity > 0)
            {
                cartItem.Quantity = quantity;
                _context.CartItems.Update(cartItem);
            }
            else
            {
                _context.CartItems.Remove(cartItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Cart/Remove
        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var cart = await GetOrCreateCartAsync();
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Cart/Checkout
        public async Task<IActionResult> Checkout()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await GetOrCreateCartAsync();
            var cartItems = cart.CartItems.ToList();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống";
                return RedirectToAction("Index");
            }

            var viewModel = new CheckoutViewModel
            {
                CartItems = cartItems,
                Total = cartItems.Sum(item => item.Quantity * item.Product.Price),
                ShippingAddress = "",
                PhoneNumber = ""
            };

            return View(viewModel);
        }

        // POST: Cart/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                TempData["Error"] = "Vui lòng đăng nhập để thanh toán";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Checkout", "Cart") });
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            try
            {
                // Kiểm tra giỏ hàng trước khi validate form
                var currentCartItems = await _context.CartItems
                    .Include(c => c.Product)
                    .Where(c => c.Cart.UserId == currentUserId)
                    .ToListAsync();

                if (!currentCartItems.Any())
                {
                    TempData["Error"] = "Giỏ hàng của bạn đang trống";
                    return RedirectToAction("Index", "Cart");
                }

                model.CartItems = currentCartItems;
                model.Total = currentCartItems.Sum(item => item.Quantity * item.Product.Price);

                // Kiểm tra form validation
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Tạo đơn hàng mới
                var order = new Order
                {
                    UserId = currentUserId,
                    OrderDate = DateTime.Now,
                    ShippingAddress = model.ShippingAddress.Trim(),
                    PhoneNumber = model.PhoneNumber.Trim(),
                    Note = model.Note?.Trim(),
                    Status = OrderStatus.Pending,
                    TotalAmount = model.Total
                };

                // Thêm chi tiết đơn hàng
                var orderDetails = currentCartItems.Select(item => new OrderDetail
                {
                    Order = order,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Product.Price,
                    TotalPrice = item.Quantity * item.Product.Price
                }).ToList();

                order.OrderDetails = orderDetails;

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Lưu đơn hàng
                        await _context.Orders.AddAsync(order);
                        
                        // Xóa giỏ hàng
                        _context.CartItems.RemoveRange(currentCartItems);
                        
                        await _context.SaveChangesAsync();
                        
                        // Commit transaction
                        await transaction.CommitAsync();
                        
                        TempData["Success"] = "Đặt hàng thành công! Cảm ơn bạn đã mua hàng.";
                        return RedirectToAction("Index", "CustomerOrders");
                    }
                    catch (Exception)
                    {
                        // Rollback nếu có lỗi
                        await transaction.RollbackAsync();
                        TempData["Error"] = "Có lỗi xảy ra khi xử lý đơn hàng. Vui lòng thử lại sau.";
                        return View(model);
                    }
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xử lý đơn hàng. Vui lòng thử lại sau.";
                return View(model);
            }
        }

        // GET: Cart/GetCartDropdown
        public IActionResult GetCartDropdown()
        {
            return PartialView("_CartDropdown");
        }
    }
}
