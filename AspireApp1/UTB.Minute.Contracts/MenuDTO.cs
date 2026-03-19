namespace UTB.Minute.Contracts;

public class MenuDto
{
    public int Id {get;set;}
    public DateTime? Date {get;set;}
    public int Portions {get;set;}

    public int MealId{get;set;}
    public string? MealName{get;set;} = string.Empty;
}