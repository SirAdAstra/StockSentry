using Domain.Enums;

namespace Domain.Entities;

public class StockMovement
{
    public int Id { get; set; }

    public int StockItemId { get; set; }
    public StockItem StockItem { get; set; } = null!;

    public int ChangeAmount { get; set; }
    public MovementType MovementType { get; set; }
    public DateTime Timestamp { get; set; }
    public string Note { get; set; } = string.Empty;
    public int PerformedByUserId { get; set; }
    public User User { get; set; } = null!;
}