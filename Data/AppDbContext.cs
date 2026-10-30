namespace BusinessDashboard.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Order>().HasIndex(o => o.OrderDate);
        b.Entity<Order>().HasIndex(o => o.Status);
        b.Entity<Order>().HasOne(o => o.Customer).WithMany().HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Order>().HasOne(o => o.Product).WithMany().HasForeignKey(o => o.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Customers.Any()) return;
        var rnd = new Random(42);
        string[] first = { "Ravi", "Anjali", "Suresh", "Priya", "Kiran", "Lakshmi", "Venkat", "Sneha", "Arjun", "Divya", "John", "Emma", "Liam", "Olivia", "Noah", "Sophia" };
        string[] last = { "Kumar", "Reddy", "Rao", "Sharma", "Patel", "Naidu", "Smith", "Johnson", "Brown", "Davis", "Miller", "Wilson" };
        string[] cities = { "Hyderabad", "Bengaluru", "Chennai", "Mumbai", "Delhi", "Pune", "London", "New York", "Sydney" };

        var customers = Enumerable.Range(1, 60).Select(i =>
        {
            var f = first[rnd.Next(first.Length)]; var l = last[rnd.Next(last.Length)];
            return new Customer
            {
                Name = f + " " + l, Email = $"{f}.{l}{i}@example.com".ToLower(),
                City = cities[rnd.Next(cities.Length)], CreatedAt = DateTime.Today.AddDays(-rnd.Next(30, 400))
            };
        }).ToList();

        (string n, string c, decimal p)[] prods =
        {
            ("Laptop Pro 14", "Electronics", 1200m), ("Smartphone X", "Electronics", 800m), ("Wireless Earbuds", "Electronics", 120m),
            ("Smart Watch", "Electronics", 250m), ("4K Monitor", "Electronics", 400m), ("Office Chair", "Furniture", 180m),
            ("Standing Desk", "Furniture", 450m), ("Bookshelf", "Furniture", 140m), ("Running Shoes", "Fashion", 90m),
            ("Leather Jacket", "Fashion", 220m), ("Backpack", "Fashion", 65m), ("Sunglasses", "Fashion", 75m),
            ("Coffee Maker", "Home", 110m), ("Air Purifier", "Home", 200m), ("Vacuum Cleaner", "Home", 160m)
        };
        var products = prods.Select(x => new Product { Name = x.n, Category = x.c, Price = x.p }).ToList();

        db.Customers.AddRange(customers);
        db.Products.AddRange(products);
        db.SaveChanges();

        string[] statuses = { "Completed", "Completed", "Completed", "Completed", "Shipped", "Pending", "Cancelled" };
        var orders = new List<Order>();
        for (int i = 0; i < 400; i++)
        {
            var prod = products[rnd.Next(products.Count)];
            var qty = rnd.Next(1, 5);
            orders.Add(new Order
            {
                CustomerId = customers[rnd.Next(customers.Count)].Id,
                ProductId = prod.Id, Quantity = qty, Amount = prod.Price * qty,
                Status = statuses[rnd.Next(statuses.Length)],
                OrderDate = DateTime.Today.AddDays(-rnd.Next(0, 365)).AddMinutes(rnd.Next(0, 1440))
            });
        }
        db.Orders.AddRange(orders);
        db.SaveChanges();
    }
}
