using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchonPM
{
    public class Project
    {
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public string ProjectOwner { get; set; } = string.Empty;
        public int ProjectID { get; set; }
        public LinkedList<Staff> Stafflist { get; set; }
        //Need to create staff object with staff name and ID
        public LinkedList<Risk> Risklist { get; set; }
        //Need to create Risk object with name, desc, catagory and risk priority
        public LinkedList<Requirement> Requirementlist { get; set; }
        //Need to create Requirement object with name, desc, id, catagory (non/function),
        public LinkedList<Deliverable> Deliverablelist { get; set; }
        //Need to create Deliverable object with name, desc, id, due date, staff assignment


        public Project(int pID, string pName, string pDescription, string pOwner)
        {
            ProjectID = pID;
            ProjectName = pName;
            ProjectDescription = pDescription;
            ProjectOwner = pOwner;
        }

        public void DisplayOverview()
        {
            Console.WriteLine($"================================"
                +$"\nProject [{ProjectID}]: {ProjectName}"
                +$"\nOwner: {ProjectOwner}"
                +$"\nDescription: {ProjectDescription}"
                +$"\n================================");

            Console.WriteLine($"\n--- Staff ({Stafflist.Count}) ---");
            foreach (var member in StaffList)
            {
                Console.WriteLine($" - {member}");
            }

            Console.WriteLine($"\n--- Risks ({Risklist.Count})");
            foreach (var risk in Risklist)
            {
                Console.WriteLine($" - [{risk.Priority}] {risk.Name}: {risk.Description}");
            }

            Console.WriteLine($"\n--- Requirements ({Requirementlist.Count}) ---");
            foreach (var req in Requirementlist)
            {
                Console.WriteLine($" - [{req.Category}] {req.Name}");
            }

            Console.WriteLine($"\n--- Deliverables ({DeliverableList.Count}) ---");
            foreach (var del in Deliverablelist)
            {
                string assigned = del.AssignedStaff != null ? del.AssignedStaff.Name : "Unassigned";
                Console.WriteLine($" - {del.Name} (Due: {del.DueDate:MM/dd/yyyy}) | Assigned: {assigned}");
            }

            Console.WriteLine($"=================================\n");
        }
    }
}
