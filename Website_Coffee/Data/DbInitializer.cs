using Microsoft.AspNetCore.Identity;
using Website_Coffee.Models;

namespace Website_Coffee.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Đảm bảo database được tạo
            context.Database.EnsureCreated();

            // Kiểm tra đã có dữ liệu chưa
            if (context.Users.Any())
            {
                return; // Đã có dữ liệu
            }

            // Tạo roles
            var roles = new[] { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Tạo admin
            var admin = new ApplicationUser
            {
                UserName = "admin@coffee.com",
                Email = "admin@coffee.com",
                FullName = "Admin",
                PhoneNumber = "0123456789",
                Address = "123 Coffee Street",
                Role = UserRole.Admin,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Tạo khách hàng mẫu
            var customers = new[]
            {
                new ApplicationUser
                {
                    UserName = "john@example.com",
                    Email = "john@example.com",
                    FullName = "John Doe",
                    PhoneNumber = "0987654321",
                    Address = "456 Customer St",
                    Role = UserRole.Customer,
                    EmailConfirmed = true
                },
                new ApplicationUser
                {
                    UserName = "jane@example.com",
                    Email = "jane@example.com",
                    FullName = "Jane Smith",
                    PhoneNumber = "0987654322",
                    Address = "789 Customer Ave",
                    Role = UserRole.Customer,
                    EmailConfirmed = true
                }
            };

            foreach (var customer in customers)
            {
                var customerResult = await userManager.CreateAsync(customer, "Customer@123");
                if (customerResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(customer, "Customer");
                }
            }

            // Tạo danh mục sản phẩm
            var categories = new[]
            {
                new Category { Name = "Cà phê", Description = "Các loại cà phê" },
                new Category { Name = "Trà", Description = "Các loại trà" },
                new Category { Name = "Bánh ngọt", Description = "Các loại bánh" }
            };

            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();

            // Tạo sản phẩm mẫu
            var products = new[]
            {
                new Product
                {
                    Name = "Cà phê đen",
                    Description = "Cà phê đen truyền thống",
                    Price = 25000,
                    CategoryId = categories[0].Id,
                    ImageUrl = "black-coffee.jpg"
                },
                new Product
                {
                    Name = "Cà phê sữa",
                    Description = "Cà phê sữa đá",
                    Price = 30000,
                    CategoryId = categories[0].Id,
                    ImageUrl = "milk-coffee.jpg"
                },
                new Product
                {
                    Name = "Trà sen",
                    Description = "Trà sen thơm mát",
                    Price = 35000,
                    CategoryId = categories[1].Id,
                    ImageUrl = "lotus-tea.jpg"
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // Tạo đơn hàng mẫu
            var orders = new[]
            {
                new Order
                {
                    UserId = customers[0].Id,
                    OrderDate = DateTime.Now.AddDays(-1),
                    Status = OrderStatus.Delivered,
                    TotalAmount = 85000,
                    ShippingAddress = customers[0].Address,
                    PhoneNumber = customers[0].PhoneNumber,
                    Note = "Giao giờ hành chính"
                },
                new Order
                {
                    UserId = customers[1].Id,
                    OrderDate = DateTime.Now.AddHours(-2),
                    Status = OrderStatus.Pending,
                    TotalAmount = 60000,
                    ShippingAddress = customers[1].Address,
                    PhoneNumber = customers[1].PhoneNumber,
                    Note = "Giao buổi sáng"
                }
            };

            context.Orders.AddRange(orders);
            await context.SaveChangesAsync();

            // Tạo chi tiết đơn hàng
            var orderDetails = new[]
            {
                new OrderDetail
                {
                    OrderId = orders[0].Id,
                    ProductId = products[0].Id,
                    Quantity = 2,
                    UnitPrice = products[0].Price,
                    TotalPrice = products[0].Price * 2
                },
                new OrderDetail
                {
                    OrderId = orders[0].Id,
                    ProductId = products[1].Id,
                    Quantity = 1,
                    UnitPrice = products[1].Price,
                    TotalPrice = products[1].Price
                },
                new OrderDetail
                {
                    OrderId = orders[1].Id,
                    ProductId = products[2].Id,
                    Quantity = 2,
                    UnitPrice = products[2].Price,
                    TotalPrice = products[2].Price * 2
                }
            };

            context.OrderDetails.AddRange(orderDetails);
            await context.SaveChangesAsync();
        }
    }
}
