namespace Domain.Entities;

public class StockItem
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public int Quantity { get; set; }

    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}