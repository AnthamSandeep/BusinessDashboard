using System.Globalization;
namespace BusinessDashboard.Controllers;

public class HomeController(AppDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var today = DateTime.Today;
        var from = new DateTime(today.Year, today.Month, 1).AddMonths(-5);

        var monthly = await db.Orders
            .Where(o => o.OrderDate >= from && o.Status != "Cancelled")
            .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Amount) })
            .ToListAsync();

        var vm = new DashboardViewModel
        {
            TotalCustomers = await db.Customers.CountAsync(),
            TotalProducts = await db.Products.CountAsync(),
            TotalOrders = await db.Orders.CountAsync(),
            Revenue = await db.Orders.Where(o => o.Status != "Cancelled").SumAsync(o => o.Amount),
            RecentOrders = await db.Orders.Include(o => o.Customer).Include(o => o.Product)
                .OrderByDescending(o => o.OrderDate).Take(8).ToListAsync()
        };

        for (int i = 0; i < 6; i++)
        {
            var m = from.AddMonths(i);
            vm.MonthLabels.Add(m.ToString("MMM yyyy", CultureInfo.InvariantCulture));
            vm.MonthRevenue.Add(monthly.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month)?.Total ?? 0);
        }

        var top = await db.Orders.Where(o => o.Status != "Cancelled")
            .GroupBy(o => o.Product!.Name)
            .Select(g => new { Name = g.Key, Revenue = g.Sum(x => x.Amount), Units = g.Sum(x => x.Quantity) })
            .OrderByDescending(x => x.Revenue).Take(5).ToListAsync();
        vm.TopProducts = top.Select(t => new TopProduct(t.Name, t.Revenue, t.Units)).ToList();

        return View(vm);
    }

    public IActionResult Error() => Content("Something went wrong. Please try again.");
}
