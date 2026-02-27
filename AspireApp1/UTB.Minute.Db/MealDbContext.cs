using Microsoft.EntityFrameworkCore;
namespace UTB.Minute.Db;


class Program
{
    static void Main(){

        var options = new DbContextOptionsBuilder<MealContext>()
                .UseSqlServer("Data Source=meals.db")
                .Options;

        using var context = new MealContext(options);
        context.Database.Migrate();
    }
    public class MealContext : DbContext{}

    List<Meal> meals =
    [
        new Meal()
        {
            Id = 1,
            Name = "Kurizek",
            Description = "Mnam",
            Price = 100,
            IsAvailable = true
        },
        new Meal()
        {
            Id = 2,
            Name = "Svickova",
            Description = "Knedlik je gud",
            Price = 89,
            IsAvailable = false
        }
    ];

    public class MealContext(DbContextOptions<MealContext> options) : DbContext(options)
    {
        public DbSet<Meal> Meals { get; set; } //tabulky v databazi
        public DbSet<Order> Orders { get; set; }
    }
}


