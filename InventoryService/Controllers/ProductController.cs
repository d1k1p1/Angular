using InventoryService.Models;
using Microsoft.AspNetCore.Mvc;


namespace InventoryService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductRepository _repository;

        public ProductController(ProductRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDTO productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Quantity = productDto.Quantity,
                Price = productDto.Price
            };
            var createdProduct = await _repository.AddProductAsync(product);
            return StatusCode(201, new { message = "Product created successfully" });
        } 

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _repository.GetProductByIdAsync(id);
            if (product == null)
                return NotFound(new { message = "Product not found" });

            return Ok(product);
        }

        public class UpdateStockRequest
        {
            public int Quantity { get; set; }
        }

        [HttpPut("{productId}/Stock")]
        public async Task<IActionResult> UpdateStock(int productId, [FromBody] UpdateStockRequest request)
        {
            var product = await _repository.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            if (product.Quantity < request.Quantity)
            {
                return BadRequest(new { message = "Not enough stock available." });
            }

            product.Quantity -= request.Quantity;
            await _repository.UpdateProductAsync(product);

            return Ok(new { message = " Product Stock updated successfully.", remainingStock = product.Quantity });
        }

        public class ProductDTO
    {
            public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
    }
}

