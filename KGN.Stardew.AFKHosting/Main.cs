using KGN.Stardew.AFKHosting.Events;
using KGN.Stardew.Framework;
using KGN.Stardew.Framework.Interfaces;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.AFKHosting
{
    public class Main : KGNMod<AFKHostingState>
    {
        public Config Config { get; private set; }

        public override void Entry(IModHelper helper)
        {
            Initialize(helper);
        }

        public void Initialize(IModHelper helper)
        {
            Config = Helper.ReadConfig<Config>();
            State = new AFKHostingState(Config.StartInAFKHostingMode);
            LoadEvents();
            HookupStardewEvents();
        }

        public void HookupStardewEvents()
        {
            InputEvents.ButtonReleased += InputEvents_ButtonReleased;
        }

        private void InputEvents_ButtonReleased(object sender, EventArgsInput e)
        {
            if (e.Button == Config.ToggleAFKKey)
                BroadcastEvent(new AFKHostingKeyPress());
        }
    }
}
