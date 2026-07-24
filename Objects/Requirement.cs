using System;

namespace ArchonPM.Objects
{
    public class Requirement
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RequirementCategory Category { get; set; }

        public Requirement(int id, string name, string desc, RequirementCategory category)
        {
            ID = id;
            Name = name;
            Description = desc;
            Category = category;
        }

        public Requirement() { }
    }
}
