using ImprovedFires.Utilities;

namespace ImprovedFires
{
    public class Main : MelonMod
    {
        internal static SaveDataManager sdm = new SaveDataManager();

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Improved Fires is online!");
            Settings.OnLoad();
        }
    }
}
