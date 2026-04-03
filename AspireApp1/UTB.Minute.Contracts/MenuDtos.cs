namespace UTB.Minute.Contracts;

public class MenuDto
{
    public int Id {get;set;}
    public DateOnly? Date {get;set;}
    public required int Portions {get;set;}

    public int MealId {get;set;}
    public string? MealName {get;set;} = string.Empty;
}

public class MenuRequestDto
{
    public DateOnly? Date {get; set;}
    public required int Portions {get; set;}
    public required int MealId {get; set;}

}