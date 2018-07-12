using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.Framework.Utilities
{
    public abstract class FunctionRateLimiter<T> : BaseRateLimiter
    {
        public T Result { get; protected set; }

        public FunctionRateLimiter(int milliseconds, Func<T> function)
            : base(milliseconds, function) { }

        protected override void Invoke()
        {
            var function = @delegate as Func<T>;
            if (function == null)
                Result = default(T);
            else
                Result = function.Invoke();

            RaiseComplete();
        }
    }
}
