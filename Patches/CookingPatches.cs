using ImprovedFires.Component;
using ImprovedFires.Enums;
using ImprovedFires.Simulation;

namespace ImprovedFires.Patches
{
    

 internal class CookingPatches
    {


        [HarmonyPatch(typeof(CookingPotItem), nameof(CookingPotItem.AttachedFireIsBurning))] //translation: attached fire meets sufficient conditions to keep cooking food

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
            public static void Prefix(CookingPotItem __instance, ref float cookTimeMinutes, ref float readyTimeMinutes)
            {
                //readyTimeMinutes is actually how long it takes for the item to burn once it's cooked
                if(__instance.m_FireBeingUsed == null) return;

                ImprovedFireComponent iFire = __instance.m_FireBeingUsed.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                if(iFire == null) return;

                CookType type = GetCookType(__instance);
            
                float rate = iFire.fireSim.CalculateCookSpeed(type);

                cookTimeMinutes /= rate;
                readyTimeMinutes /= readyTimeMinutes != 1f ? rate : 1f; //this ensures that if water is melting, once it reaches it's melted state, it gets set to boiling right away
                
            }
        }

        
         public static CookType GetCookType(CookingPotItem item)
            {
                if(item.m_LitersSnowBeingMelted.m_Units > 0f) return CookType.Snow;  //is snow
                    else if(item.m_LitersWaterBeingBoiled.m_Units > 0f) return CookType.Water; //is water
                    else return CookType.Food;
            }

       
    }

}

   