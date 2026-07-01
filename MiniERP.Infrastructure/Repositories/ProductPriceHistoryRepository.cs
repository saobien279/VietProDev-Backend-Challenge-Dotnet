using MiniERP.Application.Interfaces.Repositories;
using MiniERP.Domain.Entities;
using MiniERP.Infrastructure.Data;

namespace MiniERP.Infrastructure.Repositories
{
    public class ProductPriceHistoryRepository : IProductPriceHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductPriceHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public void Add(ProductPriceHistory history)
        {
            _context.ProductPriceHistories.Add(history);
        }
    }
}
