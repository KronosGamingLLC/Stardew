using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.Framework.Utilities
{
    public abstract class ActionRateLimiter : BaseRateLimiter
    {
        public ActionRateLimiter(int milliseconds, Action action)
            : base(milliseconds, action) { }

        protected override void Invoke()
        {
            (@delegate as Action)?.Invoke();
            RaiseComplete();
        }
    }
}
