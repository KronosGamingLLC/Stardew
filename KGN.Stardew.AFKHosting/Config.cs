using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.AFKHosting
{
    public class Config
    {
        public SButton ToggleAFKKey { get; set; }
        public bool StartInAFKHostingMode { get; set; }

        public Config()
        {
            ToggleAFKKey = SButton.H;
            StartInAFKHostingMode = false;
        }
    }
}
