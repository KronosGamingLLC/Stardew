using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.Framework.Utilities
{
    //Todo: make funtion debouncer and throttler classes 
    public class Debouncer : ActionRateLimiter
    {
        public Debouncer(int milliseconds, Action action)
            : base(milliseconds, action) { }

        public override void OnTick(object sender, EventArgs args)
        {
            timer.Stop();
            Invoke();
        }

        public override void Call()
        {
            ResetTimer();
        }
    }
}
