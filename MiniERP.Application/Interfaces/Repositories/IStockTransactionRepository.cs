using MiniERP.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MiniERP.Application.Interfaces.Repositories
{
    public interface IStockTransactionRepository
    {
        void Add(StockTransaction transaction);
        Task<IEnumerable<StockTransaction>> GetAllAsync(CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
