using Random = System.Random;

namespace ImprovedFires
{
    public class Utils
    {


        public static string GetHeatIncreaseText(float heatIncrease, bool isRadiant, Fire fire)
        {

            string text = "";
            string text2 = Localization.Get("GAMEPLAY_DegreesCUnits");
            if (GetUserMetric() == 2)
            {
                heatIncrease *= 1.8f;
                if(!isRadiant) heatIncrease += 32f;
                text2 = Localization.Get("GAMEPLAY_DegreesFUnits");
            }
            heatIncrease = fire.RoundOutput(heatIncrease);
            string symbol = heatIncrease < 0 ? "" : "+";
            if (Il2Cpp.Utils.IsZero(heatIncrease))
            {
                return text + "+0" + text2;
            }
            return text + symbol + heatIncrease + text2;
        }

        public static int GetUserMetric()
        {
            Panel_OptionsMenu Options = InterfaceManager.GetPanel<Panel_OptionsMenu>();
            if (Options == null) return -1;
            Il2CppSystem.Collections.Generic.List<GameObject> Items = Options.m_DisplayMenuItems;
            if (Items == null) return -2;
            GameObject Units = Items[3];
            if (Units == null) return -3;
            ConsoleComboBox ComboBox = Units.GetComponent<ConsoleComboBox>();
            if (ComboBox == null) return -4;
            if (ComboBox.GetCurrentIndex() == 0) return 1;
            else return 2;
        }
    }

}
