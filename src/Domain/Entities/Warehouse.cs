namespace Domain.Entities;

public class Warehouse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
}