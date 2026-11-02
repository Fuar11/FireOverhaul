using ImprovedFires.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Il2Cpp;
using System.Collections;
using Random = System.Random;

namespace ImprovedFires.Patches
{
    internal class ItemPatches : MonoBehaviour
    {
     
        [HarmonyPatch(typeof(GearItem), nameof(GearItem.Awake))]

        public class MakeTinderFuel
        {
            public static void Postfix(GearItem __instance)
            {

                GearItem gi = __instance;

                if (gi.name.Contains("GEAR_CattailTinder") ||
				gi.name.Contains("GEAR_Tinder") ||
				gi.name.Contains("GEAR_BarkTinder") ||
				gi.name.Contains("GEAR_NewsprintRoll") ||
				gi.name.Contains("GEAR_PaperStack") ||
				gi.name.Contains("GEAR_Newsprint") ||
				gi.name.Contains("GEAR_CashBundle"))
                {
                   
                    if (!gi.m_FuelSourceItem)
                    {
                        gi.m_FuelSourceItem = gi.gameObject.AddComponent<FuelSourceItem>();
                    }

                    gi.m_FuelSourceItem.m_HeatIncrease = 5; // deg C none of this really matters
                    gi.m_FuelSourceItem.m_BurnDurationHours = 0.05f;
                    gi.m_FuelSourceItem.m_IsTinder = false;
                    gi.m_FuelSourceItem.m_HeatInnerRadius = 1;
                    gi.m_FuelSourceItem.m_HeatInnerRadius = 1;

                }
            }
        }


    }
}
