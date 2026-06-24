using MiniERP.Application.Interfaces;
using System.Text.RegularExpressions;

namespace MiniERP.Application.DTOs.Categories
{
    public class UpdateCategoryRequest : INormalizable
    {
        // Id is NOT in body — comes from route
        public string CategoryName { get; set; } = null!;
        public int? ParentId { get; set; }

        public void Normalize()
        {
            CategoryName = string.IsNullOrWhiteSpace(CategoryName)
                ? string.Empty
                : Regex.Replace(CategoryName.Trim(), @"\s+", " ");
        }
    }
}
