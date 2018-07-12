using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace KGN.Stardew.Framework.Utilities
{
    public abstract class BaseRateLimiter : IDisposable
    {
        protected Timer timer;
        protected readonly double milliseconds;
        protected Delegate @delegate;
        public event EventHandler Complete;

        public BaseRateLimiter(double milliseconds, Delegate @delegate)
        {
            timer = new Timer(milliseconds);
            this.@delegate = @delegate;
            this.milliseconds = milliseconds;

            timer.Elapsed += OnTick;
        }

        protected void ResetTimer()
        {
            timer.Stop();
            timer.Start();
        }

        protected virtual void RaiseComplete()
        {
            timer.Stop();
            Complete?.Invoke(null, EventArgs.Empty);
        }

        public abstract void Call();
        protected abstract void Invoke();
        public abstract void OnTick(object sender, EventArgs args);

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    timer.Dispose();
                }

                timer = null;

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
