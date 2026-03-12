using OrderService.Models;
using OrderService.Data; // Ensure correct namespace for DbContext
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace OrderService.Repositories
{
    public class OrderRepository 
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }
      


        public async Task AddOrderAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }
    }
}
