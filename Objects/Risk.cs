using System;

namespace ArchonPM.Objects
{
    public class Risk
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public RiskPriority Priority { get; set; }
        public string Status { get; set; } = "Open";

        public Risk(string name, string desc, string category, RiskPriority priority)
        {
            Name = name;
            Description = desc;
            Category = category;
            Priority = priority;
        }

        public Risk() { }
    }
}
