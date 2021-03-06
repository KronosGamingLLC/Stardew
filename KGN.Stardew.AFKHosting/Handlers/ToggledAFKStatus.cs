﻿using KGN.Stardew.Framework;
using KGN.Stardew.Framework.Interfaces;
using KGN.Stardew.AFKHosting.Events;
using StardewModdingAPI;

namespace KGN.Stardew.AFKHosting.Handlers
{
    public class ToggledAFKStatus : EventHandler<ToggleAFKStatus, AFKHostingState>
    {
        public ToggledAFKStatus(IEventContext<ToggleAFKStatus, AFKHostingState> context) : base(context) { }

        public override AFKHostingState Execute()
        {
            var newState = context.State.With(s => s.AFKHostingOn, !context.State.AFKHostingOn);

            //TODO: maybe make some kind of watcher to fire state change events instead of executing code before the state is actually changed.          
            if (newState.AFKHostingOn) //cancel all dialogs to prepare for auto-host routines
            {
                //TODO:
                StardewAPI.EmergencyCloseDialogs();
            }
            else if(StardewAPI.CurrentDialogIsWaitForPlayers) //cancel wait for players dialog if turning hosting mode off
            {
                StardewAPI.CloseDialogOrMenu();
            }

            context.Monitor.Log($"AFK Hosting Mode toggled to {(newState.AFKHostingOn ? "on" : "off")}", LogLevel.Trace);
            return newState;
        }
    }
}
