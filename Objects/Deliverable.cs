using System;

namespace ArchonPM.Objects
{
    public class Deliverable
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Not Started";
        public DateOnly DueDate { get; set; }
        public Staff? AssignedMember { get; set; }

        public Deliverable(int id, string name, string desc, DateOnly dueDate)
        {
            ID = id;
            Name = name;
            Description = desc;
            DueDate = dueDate;
        }

        public Deliverable() { }
    }
}
