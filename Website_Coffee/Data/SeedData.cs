using Website_Coffee.Models;

namespace Website_Coffee.Data
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            if (!context.Categories.Any())
            {
                var category = new Category
                {
                    Name = "Cà phê",
                    Description = "Các loại cà phê"
                };
                context.Categories.Add(category);
                context.SaveChanges();

                var products = new[]
                {
                    new Product
                    {
                        Name = "Cà phê đen",
                        Description = "Cà phê đen đậm đà, hương vị truyền thống",
                        Price = 25000,
                        ImageUrl = "/images/products/black-coffee.jpg",
                        CategoryId = category.Id
                    },
                    new Product
                    {
                        Name = "Cà phê sữa",
                        Description = "Cà phê sữa thơm béo, hòa quyện hoàn hảo",
                        Price = 30000,
                        ImageUrl = "/images/products/milk-coffee.jpg",
                        CategoryId = category.Id
                    },
                    new Product
                    {
                        Name = "Cappuccino",
                        Description = "Cappuccino với lớp foam mịn màng, nghệ thuật latte art",
                        Price = 45000,
                        ImageUrl = "/images/products/Cappuccino.jpg",
                        CategoryId = category.Id
                    },
                    new Product
                    {
                        Name = "Espresso",
                        Description = "Espresso đậm đặc, thơm nồng",
                        Price = 35000,
                        ImageUrl = "/images/products/Espresso.jpg",
                        CategoryId = category.Id
                    },
                    new Product
                    {
                        Name = "Americano",
                        Description = "Americano thanh nhẹ, đá lạnh sảng khoái",
                        Price = 35000,
                        ImageUrl = "/images/products/Americano.jpg",
                        CategoryId = category.Id
                    },
                    new Product
                    {
                        Name = "Latte",
                        Description = "Latte thơm béo với lớp foam mềm mịn",
                        Price = 40000,
                        ImageUrl = "/images/products/Vietnamese-white-coffee.jpg",
                        CategoryId = category.Id
                    },
                    new Product
                    {
                        Name = "Mocha",
                        Description = "Mocha đậm đà vị chocolate",
                        Price = 45000,
                        ImageUrl = "/images/products/egg-coffee.jpg",
                        CategoryId = category.Id
                    },
                    new Product
                    {
                        Name = "Cold Brew",
                        Description = "Cold Brew thanh mát, ủ lạnh 24h",
                        Price = 40000,
                        ImageUrl = "/images/products/Cold-Brew.jpg",
                        CategoryId = category.Id
                    }
                };

                context.Products.AddRange(products);
                context.SaveChanges();
            }
        }
    }
}
