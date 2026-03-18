using System.ComponentModel.DataAnnotations;

namespace UTB.Minute.Db
{
    public class Meal
    {
        public int MealId { get; init; }
        [MaxLength(100)] public string? Name { get; set; }
        [MaxLength(200)] public string? Description { get; set; }
        public int? Price { get; set; }
        [MaxLength(1)] public bool IsActive { get; set; }
    }

    public class Menu //menuitem
    {
        public int MenuId { get; init; }
        public int MealId { get; init; }
        public DateTime? MenuDate { get; init; }
        public required int Portions { get; init; }

        //added
        public Meal? Meal { get; init; }
    }

    public class Order
    {
        public int OrderId { get; init; }
        public int MenuId { get; init; }
        public required OrderStatus Status { get; init; }

        //added
        public Menu? Menu { get; init; }
    }
    
    public enum OrderStatus
    {
        Preparing, //lower number of items available
        Ready, //ready to take
        Cancelled, //order does not return number of items
        Finished
    }
};
