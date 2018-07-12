namespace KGN.Stardew.AFKHosting
{
    public class AFKHostingState
    {
        public bool AFKHostingOn { get; }
        public bool WentToTodaysFestival { get; }
        
        public AFKHostingState(bool afkHostingOn, bool wentToTodaysFestival)
        {
            AFKHostingOn = afkHostingOn;
            WentToTodaysFestival = wentToTodaysFestival;
        }
    }
}
