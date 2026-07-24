using System;
using System.Collections.Generic;

namespace ArchonPM.Objects
{
    public class Staff
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<Deliverable> AssignedTasks { get; set; } = new List<Deliverable>();

        public Staff(int id, string name)
        {
            ID = id;
            Name = name;
        }

        public Staff() { }

        public override string ToString() => $"[{ID}] {Name}";
    }
}
