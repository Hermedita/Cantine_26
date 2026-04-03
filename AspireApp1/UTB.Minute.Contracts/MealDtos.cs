namespace UTB.Minute.Contracts;

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
    public required int Price {get;set;}
    public string? Description {get;set;} = string.Empty;
}

public class MealStateRequestDto
{
    public required bool IsActive { get; set; }
}