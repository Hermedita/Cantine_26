using System.ComponentModel.DataAnnotations;

namespace UTB.Minute.Db
{
    public class Meal
    {
        public int MealId { get; init; }
        [MaxLength(100)] public string? Name { get; init; }
        [MaxLength(200)] public string? Description { get; init; }
        public int? Price { get; init; }
        [MaxLength(1)] public string IsActive { get; init; } = null!;
    }

    public class Menu //menuitem
    {
        public int MenuId { get; init; }
        public int MealId { get; init; }
        public DateTime? MenuDate { get; init; }
        public required int Portions { get; init; }
    }

    public class Order
    {
        public int OrderId { get; init; }
        public int MenuId { get; init; }
        public required OrderStatus Status { get; init; }
    }
    
    public enum OrderStatus
    {
        Preparing, //lower number of items available
        Ready, //ready to take
        Cancelled, //order does not return number of items
        Finished
    }
};
