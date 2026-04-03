namespace UTB.Minute.Contracts;
public enum OrderStatus
{
    Preparing, //lower number of items available
    Ready, //ready to take
    Cancelled, //order does not return number of items
    Finished
}

public class OrderDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty; 
    public int MenuId { get; set; }
    public DateOnly? Date { get; set; }
    public string MealName { get; set; } = string.Empty;
}

public class OrderRequestDto
{
    public required int MenuId { get; set; }
}
public class OrderStatusRequestDto
{
    public required OrderStatus Status { get; set; }
}

