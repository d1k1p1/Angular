using Microsoft.AspNetCore.Mvc;
using OrderService.Data;
using OrderService.Models;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly HttpClient _httpClient;

        public OrderController(OrderDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder([FromBody] OrderDTO orderDto)
        {
            if (orderDto == null || orderDto.ProductId <= 0 || orderDto.Quantity <= 0)
                return BadRequest(new { message = "Invalid order details." });

            // Retrieve product details from InventoryService
            var productResponse = await _httpClient.GetAsync($"https://localhost:7166/api/Product/{orderDto.ProductId}");

            if (!productResponse.IsSuccessStatusCode)
                return BadRequest(new { message = "Product not available." });

            var productData = await productResponse.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<ProductDTO>(productData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (product == null)
                return BadRequest(new { message = "Invalid product response from InventoryService." });

            // Check stock availability
            if (product.Quantity < orderDto.Quantity)
                return BadRequest(new { message = "Insufficient stock for the product." });

            // Update stock in InventoryService
            var updateStockRequest = new StringContent(
                JsonSerializer.Serialize(new { quantity = orderDto.Quantity }),
                Encoding.UTF8,
                "application/json"
            );

            var updateStockResponse = await _httpClient.PutAsync($"https://localhost:7166/api/Product/{orderDto.ProductId}/Stock", updateStockRequest);

            if (!updateStockResponse.IsSuccessStatusCode)
            {
                var errorMessage = await updateStockResponse.Content.ReadAsStringAsync();
                return BadRequest(new
                {
                    message = "Failed to update stock in InventoryService.",
                    statusCode = (int)updateStockResponse.StatusCode,
                    error = errorMessage
                });
            }

            // Save the order in OrderDB
            var newOrder = new Order
            {
                ProductId = orderDto.ProductId,
                Quantity = orderDto.Quantity
            };

            _context.Orders.Add(newOrder);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Order placed successfully." });
        }
    }

    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class OrderDTO
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
