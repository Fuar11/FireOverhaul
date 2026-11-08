using ImprovedFires.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImprovedFires.Utilities;
using ImprovedFires.Simulation;
using ImprovedFires.Enums;

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

                if(fire.m_FireState != Il2Cpp.FireState.Off)
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

                    values[5] = Utils.GetHeatIncreaseText(iFire.fireSim.totalRadiantTemperature, true, fire);

                    values[6] = " - ";

                    values[7] = Utils.GetHeatIncreaseText(iFire.fireSim.totalFireTemperature, true, fire);

                    extraText = string.Concat(values);
                    
                    __result = extraText;
                }
            }
        }

        [HarmonyPatch(typeof(Panel_Cooking), nameof(Panel_Cooking.Update))]

        internal class CookingPanelCheck
        {
            

            private static void Prefix(Panel_Cooking __instance,  out bool __state)
            {

                __state = false;

                Fire fire = __instance.m_Fire;
                if (fire == null) return;

                ImprovedFireComponent iFire = fire.GetComponent<ImprovedFireComponent>();
                    if (iFire == null) return;

                if(__instance.m_Fire.m_FireState == Il2Cpp.FireState.Off && iFire.fireSim.cookingSlotTemperature > FireSimulation.GetCookMinTemp(CookType.Snow))
                {
                     fire.m_FireState = Il2Cpp.FireState.Starting_FireStarterUse;
                    __state = true;
                }
            }
            private static void Postfix(Panel_Cooking __instance, bool __state)
            {
                
                if(__instance.m_Fire != null && __state)
                {
                    __instance.m_Fire.m_FireState = Il2Cpp.FireState.Off;
                }
            }

        }

         [HarmonyPatch(typeof(Panel_CookWater), nameof(Panel_CookWater.Update))]

        internal class CookWaterPanelCheck
        {
           private static void Prefix(Panel_CookWater __instance, out bool __state)
            {
                 __state = false;

                Fire fire = __instance.m_Fire;
                if (fire == null) return;

                ImprovedFireComponent iFire = fire.GetComponent<ImprovedFireComponent>();
                    if (iFire == null) return;

                if(__instance.m_Fire.m_FireState == Il2Cpp.FireState.Off && iFire.fireSim.cookingSlotTemperature > FireSimulation.GetCookMinTemp(CookType.Snow))
                {
                     fire.m_FireState = Il2Cpp.FireState.Starting_FireStarterUse;
                    __state = true;
                }
            }
            private static void Postfix(Panel_CookWater __instance, bool __state)
            {
                
               if(__instance.m_Fire != null && __state)
                {
                    __instance.m_Fire.m_FireState = Il2Cpp.FireState.Off;
                }
            }

        }

    }
}
