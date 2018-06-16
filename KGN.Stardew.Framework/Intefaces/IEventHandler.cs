using KGN.Stardew.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.Framework.Interfaces
{
    public interface IEventHandler<TEvent,TState>
    {
        TState Handle();
    }
}
