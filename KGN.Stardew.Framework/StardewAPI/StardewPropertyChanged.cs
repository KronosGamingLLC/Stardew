using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.Framework
{
    public class StardewPropertyChangedEventArgs<T> : EventArgs
    {
        public readonly T PreviousValue;
        public readonly T CurrentValue;

        public StardewPropertyChangedEventArgs(T previousValue, T currentValue)
        {
            PreviousValue = previousValue;
            CurrentValue = currentValue;
        }
    }
}
