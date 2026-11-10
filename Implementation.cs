using FireOverhaul.Utilities;
using ComplexLogger;
using MelonLoader;

namespace FireOverhaul
{
    public class Main : MelonMod
    {
        internal static ComplexLogger<Main> Logger = new();

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Fire Overhaul is online!");
            //Settings.OnLoad();
        }
    }
}
