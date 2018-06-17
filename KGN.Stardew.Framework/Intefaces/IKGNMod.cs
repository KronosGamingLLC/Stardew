using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.Framework.Intefaces
{
    /// <summary>
    /// A mod that contains a state and can broadcast events
    /// </summary>
    /// <typeparam name="TState">A class modeling the state of the mod</typeparam>
    public interface IKGNMod<TState> : IMod
        where TState : class
    {
        TState State { get; }
        /// <summary>
        ///  Broadcast an event to be handled
        /// </summary>
        /// <param name="event">The event to broadcast</param>
        void BroadcastEvent(object @event);
    }
}
