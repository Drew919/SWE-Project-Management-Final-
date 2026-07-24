namespace DefaultNamespace;

public class Deliverable
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public Staff? AssignedStaff { get; set; }

    public Deliverable(int id, string n, string d, DateTime dd, Staff? astaff = null)
    
    {
        Id  = id;
        Name = n;
        Description = d;
        DueDate = dd;
        AssignedStaff = astaff;
    }
}