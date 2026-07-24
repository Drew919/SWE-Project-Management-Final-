using System;

namespace ArchonPM.Objects
{
    public class TimeEntry
    {
        public int ID { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public double Hours { get; set; }
        public DateTime Date { get; set; }
        public string Phase { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? RequirementId { get; set; }

        public TimeEntry(int id, string staffName, double hours, DateTime date, string phase, string desc)
        {
            ID = id;
            StaffName = staffName;
            Hours = hours;
            Date = date;
            Phase = phase;
            Description = desc;
        }

        public TimeEntry() { }
    }
}
