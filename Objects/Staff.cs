using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchonPM.Objects
{
    public class Staff
    {
        public string name { get; set; }
        public LinkedList<Deliverable> AssignedTasks { get; set; } 
    }
}
