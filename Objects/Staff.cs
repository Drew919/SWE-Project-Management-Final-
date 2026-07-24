using System.Collections.Generic;

namespace ArchonPM.Objects
{
    public class Staff
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public ProjectRole Role { get; set; }
        public LinkedList<Deliverable> AssignedTasks { get; set; } = new();
    }
}
