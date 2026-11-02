using HarmonyLib;
using Il2Cpp;
using ImprovedFires.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

namespace ImprovedFires.Patches
{
    internal class HeatSourcePatches
    {

        [HarmonyPatch(typeof(HeatSource), nameof(HeatSource.TurnOn))]

        public class HeatSourceOn
        {

            public static bool Prefix(HeatSource __instance)
            {
                string name = __instance.gameObject.name.ToLowerInvariant();
                if (name.Contains("candle") || name.Contains("flare") || name.Contains("lamp") || name.Contains("lantern") || name.Contains("torch") || name.Contains("heater"))
                {
                    return true;
                }
                else return false;

            }

            public static void Postfix(HeatSource __instance)
            {
                string name = __instance.gameObject.name.ToLowerInvariant();

                if (name.Contains("candle") || name.Contains("flare") || name.Contains("lamp") || name.Contains("lantern") || name.Contains("torch") || name.Contains("heater"))
                {
                    return;
                }

                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                __instance.m_MaxTempIncrease = iFire.totalExternalFireTemperature; //this one might not matter
                __instance.m_TempIncrease = iFire.totalExternalFireTemperature; //the current temp
                __instance.m_StartingTemp = iFire.totalExternalFireTemperature; 
            }
        }

        [HarmonyPatch(typeof(HeatSource), nameof(HeatSource.Update))]

        public class HeatSourceDisableUpdateLoop
        {
            public static bool Prefix(HeatSource __instance)
            {

                string name = __instance.gameObject.name.ToLowerInvariant();
                if (name.Contains("candle") || name.Contains("flare") || name.Contains("lamp") || name.Contains("lantern") || name.Contains("torch") || name.Contains("heater"))
                {
                    return true;
                }
                else return false;

            }

              public static void Postfix(HeatSource __instance)
            {

                string name = __instance.gameObject.name.ToLowerInvariant();
                if (name.Contains("candle") || name.Contains("flare") || name.Contains("lamp") || name.Contains("lantern") || name.Contains("torch") || name.Contains("heater"))
                {
                    return;
                }
                else
                {
                    ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>();
                    __instance.m_TempIncrease = iFire.totalExternalFireTemperature;
                }

            }
        }
    }
}
