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
    /// 
    /// </summary>
    /// <typeparam name="TEvent">The source event</typeparam>
    /// <typeparam name="TState">Must be an immutable type</typeparam>
    //TODO: add some kind of constraint to make sure T is immutable
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
