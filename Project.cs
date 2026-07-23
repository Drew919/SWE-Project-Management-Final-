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
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public string ProjectOwner { get; set; }
        public int ProjectID { get; set; }
        public LinkedList<Staff> Stafflist { get; set; }
        //Need to create staff object with staff name and ID
        public LinkedList<Risk> Risklist { get; set; }
        //Need to create Risk object with name, desc, catagory and risk priority
        public LinkedList<Requirement> Requirmentlist { get; set; }
        //Need to create Requirement object with name, desc, id, catagory (non/function),
        public LinkedList<Deliverable> Deliverablelist { get; set; }
        //Need to create Deliverable object with name, desc, id, due date, staff assignment


    }
}
