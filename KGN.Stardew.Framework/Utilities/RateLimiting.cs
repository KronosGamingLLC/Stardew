using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KGN.Stardew.Framework.Utilities
{
    public class RateLimiting
    {
        private static Dictionary<string, BaseRateLimiter> activeDebouncers = new Dictionary<string, BaseRateLimiter>();

        public static void Debounce(string key, int milliseconds, Action action)
        {
            if (activeDebouncers.ContainsKey(key))
            {
                activeDebouncers[key].Call();
            }
            else
            {
                var debouncer = new Debouncer(milliseconds, action);
                debouncer.Complete += (s, e) =>
                {
                    if (activeDebouncers.ContainsKey(key))
                    {
                        activeDebouncers[key].Dispose();
                        activeDebouncers.Remove(key);
                    }
                };
                activeDebouncers.Add(key, debouncer);
                debouncer.Call();
            }
        }
    }
}
