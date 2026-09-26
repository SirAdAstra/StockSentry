namespace Domain.Entities;

public class LowStockAlert
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    public DateTime TriggeredAt { get; set; }
    public int ThresholdAtTrigger { get; set; }
    public int QuantityAtTrigger { get; set; }
    public bool Resolved { get; set; } = false;
}