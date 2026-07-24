using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchonPM.Objects
{
    public class Project
    {
        //Class Variables
        public string Name { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public int ID { get; set; }
        public string Status { get; set; }
        public LinkedList<Staff> StaffList { get; set; }
        //Need to create staff object with staff name and assigned tasks list :Done
        public LinkedList<Risk> RiskList { get; set; }
        //Need to create Risk object with name, desc, catagory and risk priority :Done
        public LinkedList<Requirement> RequirmentList { get; set; }
        //Need to create Requirement object with name, desc, catagory (non/functional): Done
        public LinkedList<Deliverable> DeliverableList { get; set; }
        //Need to create Deliverable object with name, desc, due date, staff assignment: Done

        
        

    }
}
