using System.ComponentModel.DataAnnotations;
using UTB.Minute.Contracts;

namespace UTB.Minute.Db
{
    public class Meal
    {
        public int MealId { get; init; }
        [MaxLength(100)] public string? Name { get; set; }
        [MaxLength(200)] public string? Description { get; set; }
        public int? Price { get; set; }
        public bool IsActive { get; set; }
    }

    public class Menu //menuitem
    {
        public int MenuId { get; init; }
        public int MealId { get; set; }
        public DateOnly? MenuDate { get; set; }
        public required int Portions { get; set; }
        public Meal? Meal { get; set; }
    }

    public class Order
    {
        public int OrderId { get; set; }
        public int MenuId { get; init; }
        public required OrderStatus Status { get; set; }
        public Menu? Menu { get; set; }
    }
    
    
};
