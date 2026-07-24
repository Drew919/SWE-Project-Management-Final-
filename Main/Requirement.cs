namespace DefaultNamespace;

public class Requirement
{
    public int Id {get; set;}
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ReqCategory Category { get; set; }
}