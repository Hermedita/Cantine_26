using Microsoft.EntityFrameworkCore;

namespace UTB.Minute.Db;

public class DbContext_(DbContextOptions< DbContext_ >options): DbContext(options)
{
    public DbSet<Entity> Entities { get; set; }
}