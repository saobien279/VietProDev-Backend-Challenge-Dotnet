namespace MiniERP.Application.DTOs.Common
{
    public class PaginationQuery
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 10;
        private int _page = 1;

        public int Page
        {
            get => _page;
            set => _page = (value <= 0) ? 1 : value;
        }

        public int Limit
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : (value <= 0 ? 10 : value);
        }

        public string? Search { get; set; }
        public string? SortBy { get; set; }
        public string? SortOrder { get; set; } = "asc";
    }
}
