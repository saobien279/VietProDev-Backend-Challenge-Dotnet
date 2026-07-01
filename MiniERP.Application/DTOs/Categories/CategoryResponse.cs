namespace MiniERP.Application.DTOs.Categories
{
    public class CategoryResponse
    {
        public int Id { get; set; }
        public string CategoryName { get; set; } = null!;
        public int? ParentId { get; set; }
        public string? ParentCategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
