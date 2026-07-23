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
        //Class Variables
        public string Name { get; set; }
        public string Description { get; set; }
        public string Owner { get; set; }
        public int ID { get; set; }
        public LinkedList<Staff> StaffList { get; set; }
        //Need to create staff object with staff name and ID
        public LinkedList<Risk> RiskList { get; set; }
        //Need to create Risk object with name, desc, catagory and risk priority
        public LinkedList<Requirement> requirmentList { get; set; }
        //Need to create Requirement object with name, desc, id, catagory (non/function),
        public LinkedList<Deliverable> deliverableList { get; set; }
        //Need to create Deliverable object with name, desc, id, due date, staff assignment

        //Functions
        public Project(string ProjectName, string ProjectOwner)
        {
            Name= ProjectName; 
            Owner= ProjectOwner;
            ID= Random.Shared.Next(10000,99999);
        }
        

    }
}
