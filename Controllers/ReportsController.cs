using ClosedXML.Excel;
namespace BusinessDashboard.Controllers;

public class ReportsController(AppDbContext db) : Controller
{
    private const int PageSize = 10;

    public async Task<IActionResult> Index(string? search, string? status, DateTime? from, DateTime? to, int page = 1)
    {
        if (page < 1) page = 1;
        var q = Filter(search, status, from, to);
        var total = await q.CountAsync();
        var vm = new ReportViewModel
        {
            Items = await q.OrderByDescending(o => o.OrderDate).Skip((page - 1) * PageSize).Take(PageSize).ToListAsync(),
            Page = page, TotalCount = total, TotalPages = (int)Math.Ceiling(total / (double)PageSize),
            TotalAmount = total == 0 ? 0 : await q.SumAsync(o => o.Amount),
            Search = search, Status = status, From = from, To = to
        };
        return View(vm);
    }

    public async Task<IActionResult> Export(string? search, string? status, DateTime? from, DateTime? to)
    {
        var rows = await Filter(search, status, from, to).OrderByDescending(o => o.OrderDate).ToListAsync();

        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Orders");
        string[] headers = { "Order #", "Date", "Customer", "City", "Product", "Qty", "Amount", "Status" };
        for (int c = 0; c < headers.Length; c++)
        {
            var cell = ws.Cell(1, c + 1);
            cell.Value = headers[c];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }
        int r = 2;
        foreach (var o in rows)
        {
            ws.Cell(r, 1).Value = o.Id;
            ws.Cell(r, 2).Value = o.OrderDate;
            ws.Cell(r, 2).Style.DateFormat.Format = "dd-MMM-yyyy HH:mm";
            ws.Cell(r, 3).Value = o.Customer!.Name;
            ws.Cell(r, 4).Value = o.Customer.City;
            ws.Cell(r, 5).Value = o.Product!.Name;
            ws.Cell(r, 6).Value = o.Quantity;
            ws.Cell(r, 7).Value = o.Amount;
            ws.Cell(r, 7).Style.NumberFormat.Format = "$#,##0.00";
            ws.Cell(r, 8).Value = o.Status;
            r++;
        }
        ws.Cell(r + 1, 6).Value = "Total";
        ws.Cell(r + 1, 6).Style.Font.Bold = true;
        ws.Cell(r + 1, 7).FormulaA1 = $"SUM(G2:G{r - 1})";
        ws.Cell(r + 1, 7).Style.NumberFormat.Format = "$#,##0.00";
        ws.Cell(r + 1, 7).Style.Font.Bold = true;
        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return File(ms.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"orders-{DateTime.Now:yyyyMMdd-HHmm}.xlsx");
    }

    private IQueryable<Order> Filter(string? search, string? status, DateTime? from, DateTime? to)
    {
        var q = db.Orders.AsNoTracking().Include(o => o.Customer).Include(o => o.Product).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(o => o.Customer!.Name.Contains(search) || o.Product!.Name.Contains(search) || o.Customer.City.Contains(search));
        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(o => o.Status == status);
        if (from.HasValue) q = q.Where(o => o.OrderDate >= from.Value);
        if (to.HasValue) q = q.Where(o => o.OrderDate < to.Value.AddDays(1));
        return q;
    }
}
