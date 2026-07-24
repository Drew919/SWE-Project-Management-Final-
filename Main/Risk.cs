namespace DefaultNamespace;

public class Risk
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public RiskPriority Priority { get; set; }

    public Risk(string n, string d, string c, string p)
    {
        Name = n;
        Description = d;
        Category = c;
        Priority = p;
    }
}