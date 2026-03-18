using Microsoft.EntityFrameworkCore;

namespace UTB.Minute.Db
{

    public class MealDbContext : DbContext{
        
        public MealDbContext(DbContextOptions<MealDbContext> options) : base (options)
        {
        }
        
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Menu> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Meal>()
                .HasKey(me => me.MealId);
            
            modelBuilder.Entity<Menu>()
                .HasKey(mi => mi.MenuId);
            
            modelBuilder.Entity<Order>()
                .HasKey(o => o.OrderId);
            
            modelBuilder.Entity<Menu>()
                .HasOne<Meal>()
                .WithMany()
                .HasForeignKey(m => m.MealId);
            
            modelBuilder.Entity<Order>()
                .HasOne<Menu>()
                .WithMany()
                .HasForeignKey(o => o.MenuId);
        }
        
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlite("Data Source=minute.db");
        //}
    }
}
