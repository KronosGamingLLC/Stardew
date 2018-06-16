using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.Framework.Intefaces
{
    public interface IKGNMod<TState> : IMod
        where TState : class
    {
        TState State { get; }

        void BroadcastEvent(object @event);
    }
}
