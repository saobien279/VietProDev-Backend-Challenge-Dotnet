using MiniERP.Application.DTOs.Common;

namespace MiniERP.Application.DTOs.Products
{
    public class ProductQueryDto : PaginationQuery
    {
        public int? CategoryId { get; set; }
    }
}
