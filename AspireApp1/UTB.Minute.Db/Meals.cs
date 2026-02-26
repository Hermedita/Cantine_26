namespace UTB.Minute.Db;

public class Meal
{
    public required int Id { get; set; }
    public required string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public required double Price { get; set; }
    public required bool IsAvailable { get; set; }
    
    public ICollection<Menu> MenuItems { get; set; } = new List<Menu>();
}

public class Menu
{
    public required int Id { get; set; }
    public required DateTime Date { get; set; }
    public required int MealId { get; set; }
    public Meal Meal { get; set; } = null!;
    public required bool IsAvailable { get; set; }
    public required int Portions { get; set; }
}

public class Order
{
    public required int Id { get; set; }
    public required DateTime DateOrdered { get; set; }
    public required OrderStatus Status { get; set; }
    
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public required int Id { get; set; }
    public required int OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public required int MealId { get; set; }
    public Meal Meal { get; set; } = null!;
    public required int Portions { get; set; }
    public Menu Menu { get; set; } = null!;
}

public enum OrderStatus
{
    Preparing,      //lower number of items available
    Ready,          //ready to take
    Cancelled,      //order does not return number of items
    Finished
}