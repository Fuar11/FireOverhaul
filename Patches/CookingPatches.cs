using ImprovedFires.Component;
using ImprovedFires.Enums;
using ImprovedFires.Simulation;

namespace ImprovedFires.Patches
{
    

 internal class CookingPatches
    {


        [HarmonyPatch(typeof(CookingPotItem), nameof(CookingPotItem.AttachedFireIsBurning))] //translation: attached fire meets sufficient conditions to A) open the cooking pot panel and B) keep cooking food

        private class KeepCooking
        {
            
            private static bool Prefix(CookingPotItem __instance)
            {
                
                if(__instance.m_FireBeingUsed == null) return true;

                ImprovedFireComponent iFire = __instance.m_FireBeingUsed.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                if(iFire == null) return true;
                else return false;
            }
            private static void Postfix(CookingPotItem __instance, ref bool __result)
            {
                
                if(__instance.m_FireBeingUsed == null) return;

                ImprovedFireComponent iFire = __instance.m_FireBeingUsed.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                if(iFire == null){ 
                    return;
                }


                CookType type = CookingPatches.GetCookType(__instance);

                float min = FireSimulation.GetCookMinTemp(type);

                if(iFire.fireSim.cookingSlotTemperature > min) __result = true;
                else __result = false;
            }



        }

        [HarmonyPatch(typeof(CookingPotItem), nameof(CookingPotItem.UpdateCookingTimeAndState))]
        internal static class CookingPotItem_UpdateCookingTimeAndState {

            public static void Prefix(CookingPotItem __instance, out float __state) //__state is the rate
            {
                __state = 1f;
                //readyTimeMinutes is actually how long it takes for the item to burn once it's cooked
                if(__instance.m_FireBeingUsed == null) return;

                ImprovedFireComponent iFire = __instance.m_FireBeingUsed.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                if(iFire == null) return;

                CookType type = GetCookType(__instance);
            
                __state = iFire.fireSim.CalculateCookSpeed(type);

                if (__instance.AttachedFireIsBurning())   // only when vanilla is about to add time itself
                {
                    float dtHours = GameManager.GetTimeOfDayComponent().GetTODHours(Time.deltaTime);
                    __instance.m_CookingElapsedHours += dtHours * (__state - 1f);
                }
                
            }

            public static void Postfix(CookingPotItem __instance, float __state)
            {
                
                if(__instance.m_CookingState == CookingPotItem.CookingState.Cooking) __instance.m_MinutesUntilCooked /= __state;
                if(__instance.m_CookingState == CookingPotItem.CookingState.Ready) __instance.m_MinutesUntilRuined /= __instance.m_MinutesUntilRuined != Mathf.Infinity ? __state : 1f;

            }

        }

        [HarmonyPatch(typeof(CookingPotItem), nameof(CookingPotItem.GetHoverText))]

        internal static class DisplayCookingSlotTemp
        {
            
            private static void Postfix(CookingPotItem __instance, ref string __result)
            {
                
                if(__instance.m_FireBeingUsed == null) return;

                ImprovedFireComponent iFire = __instance.m_FireBeingUsed.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later
                if(iFire == null) return;

                __result += " - " + Utils.GetHeatIncreaseText(iFire.fireSim.cookingSlotTemperature, false, __instance.m_FireBeingUsed);
            }

        }

         public static CookType GetCookType(CookingPotItem item)
            {
                if(item.m_LitersSnowBeingMelted.m_Units > 0f) return CookType.Snow;  //is snow
                    else if(item.m_LitersWaterBeingBoiled.m_Units > 0f) return CookType.Water; //is water
                    else if(item.m_GearItemBeingCooked != null) return CookType.Food;
                    else return CookType.None;
            }

       
    }

}

   