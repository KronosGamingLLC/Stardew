using KGN.Stardew.Framework;
using KGN.Stardew.Framework.Interfaces;
using KGN.Stardew.AFKHosting.Events;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace KGN.Stardew.AFKHosting.Handlers
{
    public class AFKHostingKeyPressed : EventHandler<AFKHostingKeyPress, AFKHostingState>
    {
        public AFKHostingKeyPressed(IEventContext<AFKHostingKeyPress, AFKHostingState> context) : base(context) { }

        public override AFKHostingState Handle()
        {
            var newState = context.State.With(s => s.AFKHostingOn, !context.State.AFKHostingOn);

            //cancel active sleep dialog if turning hosting mode off
            if(!newState.AFKHostingOn)
                AFKHostingHelper.CloseDialog();

            context.Monitor.Log($"AFK Hosting Mode toggled to {(newState.AFKHostingOn ? "on" : "off")}", LogLevel.Trace);
            return newState;
        }
    }
}
