using System.Data.Entity;

namespace Coursework.Entities
{
    public class ApplicationDbContext : DbContext
    {

        public static ApplicationDbContext _instance;
        public static readonly object _lock = new object();


        private ApplicationDbContext()
            : base("name=DefaultConnection")
        {
        }

        public static ApplicationDbContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ApplicationDbContext();
                        }
                    }
                }

                return _instance;
            }
        }

        public static void Reset()
        {
            if (_instance != null)
            {
                _instance.Dispose();
                _instance = null;
            }
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Employee> Employees { get; set; } 
    }
}
