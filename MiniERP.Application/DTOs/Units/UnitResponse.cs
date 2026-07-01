namespace MiniERP.Application.DTOs.Units
{
    public class UnitResponse
    {
        public int Id { get; set; }
        public string UnitName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
