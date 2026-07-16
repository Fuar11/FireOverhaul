using HarmonyLib;
using Il2Cpp;
using ImprovedFires.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedFires.Patches
{
    internal class HeatSourcePatches
    {

        [HarmonyPatch(typeof(HeatSource), nameof(HeatSource.TurnOn))]

        public class HeatSourceOn
        {

            public static bool Prefix => false;

            public static void Postfix(HeatSource __instance)
            {
                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                __instance.m_MaxTempIncrease = iFire.totalExternalFireTemperature; //the current temp
                __instance.m_TempIncrease = iFire.totalExternalFireTemperature; 
                __instance.m_StartingTemp = iFire.totalExternalFireTemperature; 
            }
        }

        [HarmonyPatch(typeof(HeatSource), nameof(HeatSource.Update))]

        public class HeatSourceDisableUpdateLoop
        {
            public static bool Prefix => false;
        }
    }
}
