using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Diagnostics;


namespace Practice_02._09._2024_RelationshipsBetweenEntities
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using (ApplicationContext db = new ApplicationContext())
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();

                List<Company> companies = new List<Company>
                {
                new Company { Name = "Acme Corporation", Address = "123 Main St, Cityville, USA" },
                new Company { Name = "Tech Innovators Ltd.", Address = "456 Tech Blvd, Technocity, USA" },
                new Company { Name = "Global Logistics Solutions", Address = "789 Logistics Ave, Transportville, USA" },
                new Company { Name = "Green Energy Solutions Inc.", Address = "101 Eco Street, Greenburg, USA" },
                new Company { Name = "Financial Wizards LLC", Address = "202 Money Lane, Cashville, USA" }
                };

                List<Store> stores = new List<Store>
                {
                    new Store { Name = "Shop 1", Company = companies[0] },
                    new Store { Name = "Shop 2", Company = companies[1] },
                    new Store { Name = "Shop 3", Company = companies[2] },
                    new Store { Name = "Shop 4", Company = companies[3] },
                    new Store { Name = "Shop 5", Company = companies[4] }
                };


                List<User> users = new List<User>
                {
                 new User
                 {
                    FirstName = "John",
                    LastName = "Doe",
                    Age = 21,
                    Company = companies[0]
                 },
                new User
                 {
                    FirstName = "Jane",
                    LastName = "Smith",
                    Age = 30,
                    Company = companies[1]
                 },
                new User
                 {
                    FirstName = "Bob",
                    LastName = "Johnson",
                    Age = 40,
                    Company = companies[2]
                 },
                new User
                 {
                    FirstName = "Alice",
                    LastName = "Williams",
                    Age = 19,
                    Company = companies[3]
                 },
                new User
                 {
                    FirstName = "Charlie",
                    LastName = "Brown",
                    Age = 26,
                    Company = companies[4]
                 }
                };

                stores[0].StoreUsers.Add(new StoreUser { Store = stores[0], User = users[0] });
                stores[1].StoreUsers.Add(new StoreUser { Store = stores[1], User = users[1] });
                stores[2].StoreUsers.Add(new StoreUser { Store = stores[2], User = users[2] });
                stores[3].StoreUsers.Add(new StoreUser { Store = stores[3], User = users[3] });
                stores[4].StoreUsers.Add(new StoreUser { Store = stores[4], User = users[4] });

                db.Companies.AddRange(companies);
                db.Users.AddRange(users);
                db.Stores.AddRange(stores);

                db.SaveChanges();

                companies = db.Companies
                      .Include(c => c.Stores)
                      .ThenInclude(s => s.StoreUsers)
                      .ThenInclude(su => su.User)
                      .ToList();

                foreach (var current in companies)
                {
                    Console.WriteLine("Company: " + current.Name);
                    foreach (var store in current.Stores)
                    {
                        Console.WriteLine("Shop: " + store.Name);
                        foreach (var user in store.StoreUsers)
                        {
                            Console.WriteLine("User: " + user.User.FirstName + " " + user.User.LastName);
                        }
                    }
                    Console.WriteLine();
                }

                //////////////////////////////////////////////

                users = db.Users
                      .Include(u => u.StoreUser)
                      .ThenInclude(su => su.Store)
                      .ThenInclude(s => s.Company)
                      .ToList();

                foreach (var user in users)
                {
                    Console.WriteLine("User: " + user.FirstName + " " + user.LastName);
                    foreach (var store in user.StoreUser)
                    {
                        Console.WriteLine("Shop: " + store.Store.Name + " Company: " + store.Store.Company.Name);
                    }
                    Console.WriteLine();
                }

            }
        }
    }

    public class Company
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public List<Store> Stores { get; set; } = new List<Store>();
    }
    
    public class Store
    {
        public int StoreId { get; set; }
        public string? Name { get; set; }
        public int CompanyId { get; set; }
        public Company Company { get; set; }
        public List<StoreUser> StoreUsers { get; set; } = new List<StoreUser>();
    }

    public class User
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int Age { get; set; }
        public int CompanyId { get; set; }
        public virtual Company? Company { get; set; }
        public List<StoreUser> StoreUser { get; set; } = new List<StoreUser>();
    }

    public class StoreUser
    {
        public int StoreId { get; set; }
        public Store Store { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        
    }

    public class ApplicationContext : DbContext
    {
        public DbSet<Company> Companies { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<StoreUser> StoreUsers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(e => Debug.WriteLine(e), new[] { RelationalEventId.CommandExecuted });
            optionsBuilder.UseSqlServer(@"Server=DESKTOP-M496S5I;Database=Ralationships;Trusted_Connection=True; TrustServerCertificate=True");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StoreUser>()
                .HasKey(su => new { su.StoreId, su.UserId });

            modelBuilder.Entity<StoreUser>()
                .HasOne(su => su.Store)
                .WithMany(s => s.StoreUsers)
                .HasForeignKey(sc => sc.StoreId)
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<StoreUser>()
                .HasOne(su => su.User)
                .WithMany(c => c.StoreUser)
                .HasForeignKey(su => su.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
