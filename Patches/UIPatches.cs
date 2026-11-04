using ImprovedFires.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedFires.Patches
{
    internal class UIPatches
    {
        
        [HarmonyPatch(typeof(FireplaceInteraction), nameof(FireplaceInteraction.GetHoverText))]

        public class HoverTextOverride
        {

            public static void Postfix(FireplaceInteraction __instance, ref string __result)
            {

                Fire fire = __instance.Fire;
                ImprovedFireComponent iFire = fire.gameObject.GetComponent<ImprovedFireComponent>();
                string extraText = "";

                if (fire == null || iFire == null)
                {
                    Main.Logger.Log("Fire or ImprovedFireComponent is null, skipping hover text override", ComplexLogger.FlaggedLoggingLevel.Error);
                    return;
                }

                if(fire.m_FireState != FireState.Off)
                {

                    string state = "";

                    if (iFire.fireSim.state == Enums.FireState.Embers) state = "Embers";
                    else if (iFire.fireSim.state == Enums.FireState.Coals) state = "Glowing Coals";
                    else if (iFire.fireSim.state == Enums.FireState.Dwindling) state = "Dwindling Fire";
                    else if (iFire.fireSim.state == Enums.FireState.Steady) state = "Steady Fire";
                    else if (iFire.fireSim.state == Enums.FireState.Raging) state = "Raging Fire";
                    else state = "Unknown";

                    string[] values = new string[8];

                    bool displayDuration = iFire.GetDurationEstimate() >= 60f;

                    values[0] = " (";

                    values[1] = state;

                    values[2] = "\n";

                    values[3] = displayDuration ? Il2Cpp.Utils.GetDurationString((int)iFire.GetDurationEstimate() / 60) : "";

                    values[4] = displayDuration ? "\n" : "";

                    values[5] = GetHeatIncreaseText(iFire.fireSim.totalExternalFireTemperature, true, fire);

                    values[6] = " - ";

                    values[7] = GetHeatIncreaseText(iFire.fireSim.totalFireTemperature, true, fire);

                    extraText = string.Concat(values);
                    
                    __result = extraText;
                }
            }

            private static string GetHeatIncreaseText(float heatIncrease, bool includeUnits, Fire fire)
            {

                string text = "";
                string text2 = Localization.Get("GAMEPLAY_DegreesCUnits");
                if (GetUserMetric() == 2)
                {
                    heatIncrease *= 1.8f;
                    text2 = Localization.Get("GAMEPLAY_DegreesFUnits");
                }
                heatIncrease = fire.RoundOutput(heatIncrease);
                if (Il2Cpp.Utils.IsZero(heatIncrease))
                {
                    return text + "+0" + text2;
                }
                return text + "+" + heatIncrease + text2;
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
}
