using KGN.Stardew.Framework.Intefaces;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KGN.Stardew.Framework
{
    /// <summary>
    /// Custom base mod with custom event functionality and a state.
    /// </summary>
    /// <typeparam name="TState">A class modeling the state of the mod</typeparam>
    //TODO: allow for multiple handlers
    //TODO: create C# event to allow other mods to receive event broadcasts
    public abstract class KGNMod<TState> : Mod, IKGNMod<TState>
        where TState : class
    {
        private Dictionary<Type, Type> eventHandlers; //key:eventType,value:handlerType
        protected IReadOnlyDictionary<Type, Type> EventHandlers => eventHandlers;
        public TState State { get; protected set; }

        /// <summary>
        ///  Broadcasting an event automagically creates an instance of the handler with that event type and executes it.
        /// </summary>
        /// <param name="event">The event to broadcast</param>
        public void BroadcastEvent(object @event)
        {
            var eventType = @event.GetType();
            if(!eventHandlers.ContainsKey(eventType))
            {
                Monitor.Log($"Mod of type {this.GetType().Name} attempted to broadcast an event of type {eventType.Name} which it does not support.", LogLevel.Debug);
                return;
            }

            //function that creates an EventContext, passes it to a new instance of the event handler for the event, and executes the handler
            void EventRunner()
            {
                var handlerType = eventHandlers[eventType];
                var contextType = typeof(EventContext<,>).MakeGenericType(eventType, typeof(TState));

                var context = Activator.CreateInstance(contextType, new object[] { @event, State, this });
                var handler = Activator.CreateInstance(handlerType, new object[] { context });

                var state = handlerType.GetMethods().FirstOrDefault(m => m.Name == nameof(IExecutable<TState>.Execute)).Invoke(handler, new object[] { }) as TState;

                if (state != null)
                    State = state;
            }

            //TODO: run this asynchronously
            EventRunner();
        }

        /// <summary>
        /// Scans the mod assembly for event handlers and registers the event Type and handler Type in a dictionary cache
        /// </summary>
        protected void LoadEvents()
        {
            if (eventHandlers == null)
                eventHandlers = new Dictionary<Type, Type>();
            else
            {
                //release memory
                eventHandlers.Clear();
                eventHandlers = new Dictionary<Type, Type>();
            }

            var handlers = Assembly.GetAssembly(this.GetType()).GetTypes()
                .Where(t =>
                    t.GetInterfaces().Any(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(EventHandler<,>)
                        && i.GetGenericArguments()[1] == typeof(TState) //TODO: replace hardcoded generic parameter index
                ));

            foreach (var handler in handlers)
            {
                var baseType = handler.GetInterfaces().FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(EventHandler<,>));
                var eventType = baseType.GetGenericArguments()[0]; //TODO: replace hardcoded generic parameter index
                eventHandlers.Add(eventType, handler);
            }
        }
    }
}
