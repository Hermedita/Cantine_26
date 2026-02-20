using Microsoft.EntityFrameworkCore;

namespace UTB.Minute.Db;

public class DbContext_(DbContextOptions< DbContext_ >options): DbContext(options)
{
    public DbSet<Meals> Meal { get; set; } //tabulky v databazi
}