using System.Collections.Generic;

namespace ArchonPM.Objects
{
    public class Project
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public int ID { get; set; }
        public string Status { get; set; } = string.Empty;
        public LinkedList<Staff> StaffList { get; set; } = new();
        public LinkedList<Risk> RiskList { get; set; } = new();
        public LinkedList<Requirement> RequirmentList { get; set; } = new();
        public LinkedList<Deliverable> DeliverableList { get; set; } = new();
        public LinkedList<TimeEntry> TimeEntryList { get; set; } = new();
    }
}
