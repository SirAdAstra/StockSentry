namespace Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;

    public int ReorderThreshold { get; set; }
    public decimal UnitPrice { get; set; }

    public ICollection<StockItem> StockItems { get; set; } = new List<StockItem>();
}