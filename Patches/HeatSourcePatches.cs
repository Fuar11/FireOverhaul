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

                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later
                if (iFire == null)
                {
                    return true;
                }
                else return false;

            }

            public static void Postfix(HeatSource __instance)
            {
                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                if (iFire == null)
                {
                    //Main.Logger.Log("iFire is null when trying to add heat source.", ComplexLogger.FlaggedLoggingLevel.Error);
                    return;
                }

                GameManager.GetHeatSourceManagerComponent().AddHeatSource(__instance);

                __instance.m_MaxTempIncrease = iFire.fireSim.totalExternalFireTemperature; //this one might not matter
                __instance.m_TempIncrease = iFire.fireSim.totalExternalFireTemperature; //the current temp
                __instance.m_StartingTemp = iFire.fireSim.totalExternalFireTemperature; 

                __instance.m_TurnedOn = true;
            }
        }

        [HarmonyPatch(typeof(HeatSource), nameof(HeatSource.Update))]

        public class HeatSourceDisableUpdateLoop
        {
            public static bool Prefix(HeatSource __instance)
            {

                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later
                if (iFire == null)
                {
                    return true;
                }
                else return false;

            }

              public static void Postfix(HeatSource __instance)
            {

                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                if (iFire == null)
                {
                    return;
                }
                else
                {
                    __instance.m_TempIncrease = iFire.fireSim.totalExternalFireTemperature;
                    __instance.m_MaxTempIncreaseOuterRadius = iFire.fireSim.outerRadius;
                    __instance.m_MaxTempIncreaseInnerRadius = iFire.fireSim.innerRadius;
                }

            }
        }
    }
}
