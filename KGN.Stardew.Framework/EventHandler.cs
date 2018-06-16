using KGN.Stardew.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace KGN.Stardew.Framework
{
    public abstract class EventHandler<TEvent, TState> : IEventHandler<TEvent, TState>
    {
        protected readonly IEventContext<TEvent, TState> context;

        public EventHandler(IEventContext<TEvent, TState> context)
        {
            this.context = context;
        }

        public abstract TState Handle();
    }
}
