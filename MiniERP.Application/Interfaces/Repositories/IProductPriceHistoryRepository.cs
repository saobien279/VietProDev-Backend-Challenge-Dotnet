using MiniERP.Domain.Entities;

namespace MiniERP.Application.Interfaces.Repositories
{
    public interface IProductPriceHistoryRepository
    {
        void Add(ProductPriceHistory history);
    }
}
