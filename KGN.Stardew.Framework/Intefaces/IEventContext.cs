using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.Framework.Interfaces
{
    public interface IEventContext<TEvent, TState>
    {
        TState State { get; }
        TEvent Event { get; }
        IMonitor Monitor { get; }
    }
}
