using StardewModdingAPI;

namespace KGN.Stardew.Framework.Interfaces
{
    /// <summary>
    /// A context to pass to an event handler that exposes state, holds a copy of the original event, and exposes certain mod functionality
    /// </summary>
    /// <typeparam name="TEvent">The source event</typeparam>
    /// <typeparam name="TState">The state model class being exposed, usually the state model class of the mod</typeparam>
    public interface IEventContext<TEvent, TState>
        where TState : class
    {
        TState State { get; }
        TEvent Event { get; }
        IMonitor Monitor { get; }
        IModHelper Helper { get; }
    }
}
