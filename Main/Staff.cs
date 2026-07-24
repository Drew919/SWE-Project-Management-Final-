namespace DefaultNamespace;

public class Staff
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public Staff(int id, string name)
    {
        ID = id;
        Name = name;
    }

    public override string ToString() => $"[ID: {Id}, Name: {Name}]}";

}