using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchonPM
{
    public class Staff
    {
        public string name { get; set; }
        public int id { get; set; }
        public LinkedList<Deliverable> assignedTasks { get; set; } 
    }
}
