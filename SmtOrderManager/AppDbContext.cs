using Microsoft.EntityFrameworkCore;

namespace SmtOrderManager
{
    public class AppDbContext : DbContext
    {
        private readonly string _connectionString;

        public AppDbContext(string connectionString)
        {
            _connectionString = connectionString;

        }

        public DbSet<Component> Components => Set<Component>();
        public DbSet<Board> Boards => Set<Board>();
        public DbSet<Order> Orders => Set<Order>();

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(_connectionString);

        protected override void OnModelCreating(ModelBuilder model)
        {
            model.Entity<Board>()
                .HasMany(b => b.Components)
                .WithMany(c => c.Boards)
                .UsingEntity(j => j.ToTable("BoardComponents"));

            model.Entity<Order>()
                .HasMany(o => o.Boards)
                .WithMany(b => b.Orders)
                .UsingEntity(j => j.ToTable("OrderBoards"));
        }
    }
}