namespace Balancio.Models;

public class Category
{
    public string Name { get; set; } = string.Empty;

    public decimal MonthlyAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public List<MonthlyAmount> History { get; set; } = new();

    public Color ColorHex { get; set; } = Colors.Orange;
}

public class MonthlyAmount
{
    public string Month { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}