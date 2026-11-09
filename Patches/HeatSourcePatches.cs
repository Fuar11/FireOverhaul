using HarmonyLib;
using Il2Cpp;
using FireOverhaul.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.RenderGraphModule.NativeRenderPassCompiler;

namespace FireOverhaul.Patches
{
    internal class HeatSourcePatches
    {

        [HarmonyPatch(typeof(HeatSource), nameof(HeatSource.TurnOn))]

        public class HeatSourceOn
        {

            public static bool Prefix(HeatSource __instance)
            {

                FireOverhaulComponent iFire = __instance.gameObject.GetComponent<FireOverhaulComponent>(); //for now, might use GUID later

                if (iFire == null)
                {
                    //Main.Logger.Log("iFire is null when trying to add heat source.", ComplexLogger.FlaggedLoggingLevel.Error);
                    return true;
                }

                GameManager.GetHeatSourceManagerComponent().AddHeatSource(__instance);

                __instance.m_MaxTempIncrease = iFire.fireSim.totalRadiantTemperature; //this one might not matter
                __instance.m_TempIncrease = iFire.fireSim.totalRadiantTemperature; //the current temp
                __instance.m_StartingTemp = iFire.fireSim.totalRadiantTemperature;

                __instance.m_TurnedOn = true;

                return false;
            }
        }

        [HarmonyPatch(typeof(HeatSource), nameof(HeatSource.Update))]

        public class HeatSourceDisableUpdateLoop
        {
            public static bool Prefix(HeatSource __instance)
            {
                FireOverhaulComponent iFire = __instance.gameObject.GetComponent<FireOverhaulComponent>(); //for now, might use GUID later

                if (iFire == null)
                {
                    return true;
                }
                else
                {
                    __instance.m_TempIncrease = iFire.fireSim.totalRadiantTemperature;
                    __instance.m_MaxTempIncreaseOuterRadius = iFire.fireSim.outerRadius;
                    __instance.m_MaxTempIncreaseInnerRadius = iFire.fireSim.innerRadius;
                }

                return false;
            }
        }
    }
}
