using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProductsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private static List<Product> _products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 10.99m, Description = "Description 1", Category = "Category 1" },
            new Product { Id = 2, Name = "Product 2", Price = 19.99m, Description = "Description 2", Category = "Category 2" },
            new Product { Id = 3, Name = "Product 3", Price = 7.5m, Description = "Description 3", Category = "Category 1" }
        };

        private static List<Category> _categories = new List<Category>
        {
            new Category { Id = 1, Name = "Category 1" },
            new Category { Id = 2, Name = "Category 2" },
            new Category { Id = 3, Name = "Category 3" }
        };

        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ILogger<ProductsController> logger)
        {
            _logger = logger;
        }

        // GET: api/products
        [HttpGet]
        public IEnumerable<Product> Get()
        {
            return _products;
        }

        // GET: api/products/{id}
        [HttpGet("{id}")]
        public ActionResult<Product> Get(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return product;
        }
        private string SaveImage(IFormFile file)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "images");
            var filePath = Path.Combine(directoryPath, fileName);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(fileStream);
            }

            return filePath;
        }

        private void DeleteImage(string imagePath)
        {
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
        }
        private int GenerateProductId()
        {
            return _products.Count > 0 ? _products.Max(p => p.Id) + 1 : 1;
        }
        // POST: api/products
        [HttpPost]
        public ActionResult<Product> Post([FromForm] ProductFormModel formModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = new Product
            {
                Id = GenerateProductId(),
                Name = formModel.Name,
                Price = formModel.Price,
                Description = formModel.Description,
                Category = formModel.Category
            };

            if (formModel.Image != null)
            {
                product.ImagePath = SaveImage(formModel.Image);
            }

            _products.Add(product);

            return CreatedAtAction("Get", new { id = product.Id }, product);
        }

        // PUT: api/products/{id}
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromForm] ProductFormModel formModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingProduct = _products.FirstOrDefault(p => p.Id == id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            existingProduct.Name = formModel.Name;
            existingProduct.Price = formModel.Price;
            existingProduct.Description = formModel.Description;
            existingProduct.Category = formModel.Category;

            if (formModel.Image != null)
            {
                if (!string.IsNullOrEmpty(existingProduct.ImagePath))
                {
                    DeleteImage(existingProduct.ImagePath);
                }
                existingProduct.ImagePath = SaveImage(formModel.Image);
            }

            return NoContent();
        }

        // DELETE: api/products/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.ImagePath))
            {
                DeleteImage(product.ImagePath);
            }

            _products.Remove(product);
            return NoContent();
        }

        // GET: api/categories
        [HttpGet("categories")]
        public IEnumerable<Category> GetCategories()
        {
            return _categories;
        }

        // GET: api/categories/{id}
        [HttpGet("categories/{id}")]
        public ActionResult<Category> GetCategory(int id)
        {
            var category = _categories.FirstOrDefault(c => c.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return category;
        }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ImagePath { get; set; }
    }

    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ProductFormModel
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public IFormFile Image { get; set; }
    }
}