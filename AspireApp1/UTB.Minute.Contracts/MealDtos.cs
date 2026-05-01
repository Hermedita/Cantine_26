namespace UTB.Minute.Contracts;
using System.ComponentModel.DataAnnotations;
public class MealDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    
    public required int Price { get; set; }
    public string? Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class MealRequestDto
{
    public required string Name {get;set;}
    public string? Description {get;set;} = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "Price cannot be less than 0.")]
    public required int Price {get;set;}
}
public class MealStateRequestDto
{
    public required bool IsActive { get; set; }
}