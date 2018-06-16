using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.AFKHosting
{
    public class AFKHostingState
    {
        public bool AFKHostingOn { get; }
        
        public AFKHostingState(bool afkHostingOn)
        {
            AFKHostingOn = afkHostingOn;
        }
    }
}
