using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.Advertisement;

namespace ArchonPM.Objects
{
    public class Deliverable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateOnly DueDate { get; set; }
        public Staff AssignedMember { get; set; }
        
    }
}
