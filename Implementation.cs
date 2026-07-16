using ImprovedFires.Utilities;
using ComplexLogger;
using MelonLoader;

namespace ImprovedFires
{
    public class Main : MelonMod
    {
        internal static SaveDataManager sdm = new SaveDataManager();
        internal static ComplexLogger<Main> Logger = new();

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Improved Fires is online!");
            Settings.OnLoad();
        }
    }
}
