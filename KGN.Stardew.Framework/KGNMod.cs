using KGN.Stardew.Framework.Intefaces;
using KGN.Stardew.Framework.Interfaces;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.Framework
{
    public abstract class KGNMod<TState> : Mod, IKGNMod<TState>
        where TState : class
    {
        private Dictionary<Type, Type> events;
        protected IReadOnlyDictionary<Type, Type> Events => events;
        public TState State { get; protected set; }

        public void BroadcastEvent(object @event)
        {
            var eventType = @event.GetType();
            if(!events.ContainsKey(eventType))
            {
                Monitor.Log($"Mod of type {this.GetType().Name} attempted to broadcast an event of type {eventType.Name} which it does not support.", LogLevel.Debug);
                return;
            }

            void EventRunner()
            {
                var handlerType = events[eventType];
                var contextType = typeof(EventContext<,>).MakeGenericType(eventType, typeof(TState));

                var context = Activator.CreateInstance(contextType, new object[] { @event, State, this });
                var handler = Activator.CreateInstance(handlerType, new object[] { context });

                var state = handlerType.GetMethods().FirstOrDefault(m => m.Name == "Handle").Invoke(handler, new object[] { }) as TState;

                if (state != null)
                    State = state;
            }

            //TODO: run this asynchronously
            EventRunner();
        }

        protected void LoadEvents()
        {
            if (events == null)
                events = new Dictionary<Type, Type>();
            else
            {
                events.Clear();
                events = new Dictionary<Type, Type>();
            }

            var handlers = Assembly.GetAssembly(this.GetType()).GetTypes()
                .Where(t =>
                    t.GetInterfaces().Any(i =>
                        i.IsGenericType
                        && i.GetGenericTypeDefinition() == typeof(IEventHandler<,>)
                        && i.GetGenericArguments()[1] == typeof(TState)
                ));

            foreach (var handler in handlers)
            {
                var interfaceType = handler.GetInterfaces().FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEventHandler<,>));
                var eventType = interfaceType.GetGenericArguments()[0];
                events.Add(eventType, handler);
            }
        }
    }
}
