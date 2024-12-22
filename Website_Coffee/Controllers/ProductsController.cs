using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Website_Coffee.Data;
using Website_Coffee.Models;

namespace Website_Coffee.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Categories"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "products");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    Directory.CreateDirectory(uploadsFolder);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    product.ImageUrl = "/images/products/" + uniqueFileName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["Categories"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewData["Categories"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile imageFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null)
                    {
                        string uploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "images", "products");
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        Directory.CreateDirectory(uploadsFolder);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(fileStream);
                        }

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            string oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        product.ImageUrl = "/images/products/" + uniqueFileName;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["Categories"] = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Delete image if exists
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    string imagePath = Path.Combine(_hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Products/UpdateImages
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateImages()
        {
            await SeedCategories(); // Thêm categories trước
            await SeedProducts(); // Sau đó thêm products
            return RedirectToAction(nameof(Index));
        }

        private async Task SeedCategories()
        {
            if (!_context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Cà phê", Description = "Các loại cà phê" },
                    new Category { Name = "Trà", Description = "Các loại trà" }
                };

                _context.Categories.AddRange(categories);
                await _context.SaveChangesAsync();
            }
        }

        private async Task SeedProducts()
        {
            // Xóa sản phẩm cũ nếu có
            var existingProducts = await _context.Products.ToListAsync();
            if (existingProducts.Any())
            {
                _context.Products.RemoveRange(existingProducts);
                await _context.SaveChangesAsync();
            }

            // Lấy ID của categories
            var cafeCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Cà phê");
            var teaCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == "Trà");

            if (cafeCategory == null || teaCategory == null)
            {
                throw new InvalidOperationException("Categories must be seeded first");
            }

            var products = new List<Product>
            {
                new Product
                {
                    Name = "Cà phê đen",
                    Description = "Cà phê đen truyền thống Việt Nam",
                    Price = 25000,
                    ImageUrl = "black-coffee.jpg",
                    CategoryId = cafeCategory.Id,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Cà phê sữa",
                    Description = "Cà phê sữa đá thơm ngon",
                    Price = 30000,
                    ImageUrl = "milk_coffee.jpg",
                    CategoryId = cafeCategory.Id,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Trà sen",
                    Description = "Trà sen thanh mát",
                    Price = 35000,
                    ImageUrl = "Vietnamese white coffee.jpg", // Tạm thởi dùng ảnh này
                    CategoryId = teaCategory.Id,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Cappuccino",
                    Description = "Cappuccino Ý",
                    Price = 45000,
                    ImageUrl = "Cappuccino.jpg",
                    CategoryId = cafeCategory.Id,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Trà đào",
                    Description = "Trà đào thơm ngon",
                    Price = 35000,
                    ImageUrl = "espresso.jpg", // Tạm thởi dùng ảnh này cho Trà đào
                    CategoryId = teaCategory.Id,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Americano",
                    Description = "Cà phê Americano thơm ngon",
                    Price = 35000,
                    ImageUrl = "Americano.jpg",
                    CategoryId = cafeCategory.Id,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Espresso",
                    Description = "Espresso đậm đà",
                    Price = 35000,
                    ImageUrl = "espresso.jpg", 
                    CategoryId = cafeCategory.Id,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Cold Brew",
                    Description = "Cà phê ủ lạnh",
                    Price = 40000,
                    ImageUrl = "Cold-Brew.jpg",
                    CategoryId = cafeCategory.Id,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Egg Coffee",
                    Description = "Cà phê trứng Hà Nội",
                    Price = 45000,
                    ImageUrl = "egg coffee.jpg",
                    CategoryId = cafeCategory.Id,
                    IsAvailable = true
                },
                new Product
                {
                    Name = "Salt Coffee",
                    Description = "Cà phê muối độc đáo",
                    Price = 40000,
                    ImageUrl = "salt coffee.jpg",
                    CategoryId = cafeCategory.Id,
                    IsAvailable = true
                }
            };

            _context.Products.AddRange(products);
            await _context.SaveChangesAsync();
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
