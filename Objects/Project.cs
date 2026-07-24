using System;
using System.Collections.Generic;

namespace ArchonPM.Objects
{
    public class Project
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public int ID { get; set; }
        public string Status { get; set; } = "Planning";

        public List<Staff> StaffList { get; set; } = new List<Staff>();
        public List<Risk> RiskList { get; set; } = new List<Risk>();
        public List<Requirement> RequirementList { get; set; } = new List<Requirement>();
        public List<Deliverable> DeliverableList { get; set; } = new List<Deliverable>();
    }
}
