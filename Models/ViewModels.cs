namespace BusinessDashboard.Models;

public record TopProduct(string Name, decimal Revenue, int Units);

public class DashboardViewModel
{
    public int TotalCustomers { get; set; }
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public decimal Revenue { get; set; }
    public List<Order> RecentOrders { get; set; } = new();
    public List<string> MonthLabels { get; set; } = new();
    public List<decimal> MonthRevenue { get; set; } = new();
    public List<TopProduct> TopProducts { get; set; } = new();
}

public class ReportViewModel
{
    public List<Order> Items { get; set; } = new();
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Search { get; set; }
    public string? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}
