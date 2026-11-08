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
				gi.name.Contains("GEAR_CashBundle") ||
                gi.name.Contains("GEAR_Cloth") ||
				gi.name.Contains("GEAR_OldMansBeardHarvested"))
                {
                   
                    if (!gi.m_FuelSourceItem)
                    {
                        gi.m_FuelSourceItem = gi.gameObject.AddComponent<FuelSourceItem>();
                    }

                    gi.m_FuelSourceItem.m_FireStartSkillModifier = GetStartSkillBonusByName(gi.name);
                    gi.m_FuelSourceItem.m_BurnDurationHours = 0.05f;
                    gi.m_FuelSourceItem.m_IsTinder = false;
                    gi.m_FuelSourceItem.m_HeatInnerRadius = 1;
                    gi.m_FuelSourceItem.m_HeatInnerRadius = 1;

                }
            }

            private static float GetStartSkillBonusByName(string name)
            {
                switch (name)
                {
                    case "GEAR_CattailTinder": return 40f;
                    case "GEAR_Tinder": return 25f;
                    case "GEAR_BarkTinder": return 30f;
                    case "GEAR_NewsprintRoll":
                    case "GEAR_Newsprint": return 35f;
                    case "GEAR_PaperStack": return 30f;
                    case "GEAR_CashBundle": return 35f;
                    case "GEAR_Cloth": return 5f;
                    case "GEAR_OldMansBeardHarvested": return 40f;
                    default: return 10f;

                }
            }

        }

        [HarmonyPatch(typeof(Skill_Firestarting), nameof(Skill_Firestarting.TinderRequired))]

        internal class DontUseTinderAsTinder
        {
            
            private static void Postfix(ref bool __result){
                __result = false;
            }

        }

      

    }
}
