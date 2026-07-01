using System;
using System.Threading.Tasks;

namespace MiniERP.Application.Interfaces.Services
{
    public interface IDailySummaryJob
    {
        Task ExecuteAsync(DateTime? targetDate = null);
    }
}
