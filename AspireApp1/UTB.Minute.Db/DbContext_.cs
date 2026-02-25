using Microsoft.EntityFrameworkCore;

namespace UTB.Minute.Db;

public class DbContext_(DbContextOptions< DbContext_ >options): DbContext(options)
{
    public DbSet<Meal> Meal { get; set; } //tabulky v databazi
}