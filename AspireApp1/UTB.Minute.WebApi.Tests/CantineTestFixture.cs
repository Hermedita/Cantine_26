using Aspire.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using UTB.Minute.Db;
using Aspire.Hosting.Testing;
using Xunit;
using UTB.Minute.Contracts;

namespace UTB.Minute.WebApi.Tests
{

    public class CantineTestFixture : IAsyncLifetime
    {
        private DistributedApplication app = null!;
        private string? connectionString;
        public HttpClient HttpClient { get; private set; } = null!;

        public async ValueTask InitializeAsync()
        {
            var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.UTB_Minute_AppHost>(["--environment=Testing"], TestContext.Current.CancellationToken);

            app = await builder.BuildAsync(TestContext.Current.CancellationToken);
            await app.StartAsync(TestContext.Current.CancellationToken);


            await app.ResourceNotifications.WaitForResourceHealthyAsync("database", TestContext.Current.CancellationToken);
            await app.ResourceNotifications.WaitForResourceHealthyAsync("web-api", TestContext.Current.CancellationToken);

            connectionString = await app.GetConnectionStringAsync("database", TestContext.Current.CancellationToken);
            
            HttpClient = app.CreateHttpClient("web-api", "https");

            using var context = CreateContext();

            await context.Database.EnsureDeletedAsync(TestContext.Current.CancellationToken);
            await context.Database.EnsureCreatedAsync(TestContext.Current.CancellationToken);

            Meal rizek = new() { Name = "Kuřecí řízek", Price = 135, IsActive = true, Description = "Smažený řízek" };
            Meal smazak = new() { Name = "Smažák", Price = 120, IsActive = true, Description = "Sýr" };

            Menu rizekMenu = new() {Meal = rizek,MenuDate = new DateOnly(2026, 4, 6),Portions = 50};
            Menu smazakMenu = new() {Meal = smazak,MenuDate = new DateOnly(2026, 4, 7),Portions = 0};

            Order rizekOrder = new Order() {Menu = rizekMenu, Status = OrderStatus.Preparing};

            context.Meals.AddRange(rizek, smazak);
            context.MenuItems.AddRange(rizekMenu, smazakMenu);
            context.Orders.Add(rizekOrder);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            HttpClient.Dispose();
            await app.DisposeAsync();
            GC.SuppressFinalize(this);
        }

        public MealDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<MealDbContext>()
                    .UseSqlServer(connectionString) 
                    .Options;

            return new MealDbContext(options);
        }
    }

    [CollectionDefinition("Cantine Collection", DisableParallelization = true)]
    public class CantineCollection : ICollectionFixture<CantineTestFixture>
    {
    }
}