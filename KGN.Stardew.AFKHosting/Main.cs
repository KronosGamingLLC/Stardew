using KGN.Stardew.AFKHosting.Events;
using KGN.Stardew.Framework;
using KGN.Stardew.Framework.Interfaces;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
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
            Config = Helper.ReadConfig<Config>();
            State = new AFKHostingState(Config.StartInAFKHostingMode);
            LoadEvents();

            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            SaveEvents.AfterCreate += SaveEvents_AfterCreate;

            SaveEvents.AfterReturnToTitle += (s, e) =>
            {
                SaveEvents.AfterLoad += SaveEvents_AfterLoad;
                SaveEvents.AfterCreate += SaveEvents_AfterCreate;
            };
        }

        private void SaveEvents_AfterCreate(object sender, EventArgs e)
        {
            Initialize();
        }

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            Initialize();
        }

        private void Initialize()
        {
            SaveEvents.AfterLoad -= SaveEvents_AfterLoad;
            SaveEvents.AfterCreate -= SaveEvents_AfterCreate;

            if (Context.IsMultiplayer)
                HookupStardewEvents();
            else
                Monitor.Log("AFKHosting disabled in single player mode.", LogLevel.Info);
        }

        public void HookupStardewEvents()
        {
            InputEvents.ButtonReleased += InputEvents_ButtonReleased;
            GameEvents.QuarterSecondTick += GameEvents_QuarterSecondTick;
        }

        public void ReleaseStardewEvents()
        {
            InputEvents.ButtonReleased -= InputEvents_ButtonReleased;
            GameEvents.QuarterSecondTick -= GameEvents_QuarterSecondTick;
        }

        private void GameEvents_QuarterSecondTick(object sender, EventArgs e)
        {
            if (Context.IsWorldReady
                && State.AFKHostingOn
                && Context.IsPlayerFree
                && AFKHostingHelper.RemotePlayersOnline
                && AFKHostingHelper.PlayerInBed)
            {
                AFKHostingHelper.StartSleep(Helper);
            }
        }

        private void InputEvents_ButtonReleased(object sender, EventArgsInput e)
        {
            if (Context.IsWorldReady && e.Button == Config.ToggleAFKKey)
                BroadcastEvent(new AFKHostingKeyPress());
        }
    }
}
