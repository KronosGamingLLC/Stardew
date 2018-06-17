using StardewModdingAPI;

namespace KGN.Stardew.AFKHosting
{
    public class Config
    {
        public SButton ToggleAFKKey { get; set; }
        public bool StartInAFKHostingMode { get; set; }

        public Config()
        {
            ToggleAFKKey = SButton.H;
            StartInAFKHostingMode = false;
        }
    }
}
