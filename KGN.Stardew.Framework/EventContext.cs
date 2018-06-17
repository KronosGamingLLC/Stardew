using KGN.Stardew.Framework.Intefaces;
using KGN.Stardew.Framework.Interfaces;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.Framework
{
    /// <summary>
    /// The context to pass to an event handler that exposes state, holds a copy of the original event, and exposes certain mod functionality
    /// </summary>
    /// <typeparam name="TEvent">The source event</typeparam>
    /// <typeparam name="TState">The state model class being exposed, usually the state model class of the mod</typeparam>
    public class EventContext<TEvent, TState> : IEventContext<TEvent,TState>
        where TState : class
    {
        private readonly IKGNMod<TState> mod;
        public TState State { get; private set; }
        public TEvent Event { get; }
        public IModHelper Helper => mod.Helper;
        public IMonitor Monitor => mod.Monitor;
        
        public EventContext(TEvent @event, TState state, IKGNMod<TState> mod)
        {
            State = state;
            Event = @event;
            this.mod = mod;
        }
    }
}
