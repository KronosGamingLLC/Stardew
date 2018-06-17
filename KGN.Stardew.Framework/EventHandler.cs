using KGN.Stardew.Framework.Intefaces;
using KGN.Stardew.Framework.Interfaces;

namespace KGN.Stardew.Framework
{
    /// <summary>
    /// Base class for an event handler
    /// </summary>
    /// <typeparam name="TEvent">The event being handled</typeparam>
    /// <typeparam name="TState">The state model class of the context, usually the state model class of the mod.</typeparam>
    public abstract class EventHandler<TEvent, TState> : IExecutable<TState>
        where TState : class
    {
        protected readonly IEventContext<TEvent, TState> context;

        public EventHandler(IEventContext<TEvent, TState> context)
        {
            this.context = context;
        }

        public abstract TState Execute();
    }
}
