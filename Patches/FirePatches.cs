using Il2CppTLD.Interactions;
using ImprovedFires.Component;
using ImprovedFires.Enums;
using ImprovedFires.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedFires.Patches
{
    internal class FirePatches
    {

        [HarmonyPatch(typeof(Fire), nameof(Fire.Awake))]

        public class AddFireComponent
        {

            public static void Postfix(Fire __instance)
            {

                if (__instance.gameObject)
                {

                    if(__instance.gameObject.name.ToLowerInvariant().Contains("invisiblefire")) return;

                    Main.Logger.Log("Adding fire component to fire object, wahoo!", ComplexLogger.FlaggedLoggingLevel.Debug);
                    __instance.gameObject.AddComponent<ImprovedFireComponent>();
                }

                GameManager.GetFireManagerComponent().m_MaxDurationHoursOfFire = float.PositiveInfinity;


            }

        }

        [HarmonyPatch(typeof(Fire), nameof(Fire.Update))]

        public class DisableUpdateLoop
        {
            public static bool Prefix(Fire __instance)
            {
                
                if(__instance.gameObject.name.ToLowerInvariant().Contains("invisiblefire")) return true;
                else return false;

            }
            public static void Postfix(Fire __instance)
            {

                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                if (GameManager.m_IsPaused || GameManager.s_IsGameplaySuspended || iFire == null)
                {
                    return;
                }

                if (__instance.m_TriggerStageRequested)
                {
                    __instance.m_FX.TriggerStage(__instance.m_FireState, shouldPlay: true);
                    __instance.m_TriggerStageRequested = false;
                }
                if (__instance.m_FireState == Il2Cpp.FireState.Off) //completely dead, all of this might need changing too
                {
                    __instance.m_TimeOffSeconds += Time.deltaTime; //real world seconds
                    if (__instance.m_TimeOffSeconds > 2f)
                    {
                        Il2Cpp.Utils.SetActive(__instance.m_FX.lighting.gameObject, false);
                        __instance.m_FX.DeactivateAllFX();
                        __instance.m_TimeOffSeconds = float.NegativeInfinity;
                    }
                    return;
                }

                __instance.UpdateFireStage(); 
                //__instance.MaybeBlowOutFromWind(); this needs a patch
                
                if(iFire.fireSim.state == Enums.FireState.Starting) __instance.UpdateFireAudio();
                else iFire.fireSim.CustomFireAudio(__instance);

                //needed for lighting
                __instance.m_ElapsedOnTODSeconds = 0f;
                __instance.m_MaxOnTODSeconds = iFire.GetDurationEstimate();

                if (iFire.fireSim.state == Enums.FireState.Raging && iFire.changeFX)
                {
                    __instance.m_FX.TriggerStage(Il2Cpp.FireState.FullBurn, true, true); //need something else here
                    iFire.changeFX = false;
                }
                if (iFire.fireSim.state == Enums.FireState.Steady && iFire.changeFX)
                {
                    Il2Cpp.FireState visualState = Il2Cpp.FireState.Starting_MediumBurn;
                    if(iFire.fireSim.type == FireType.Fireplace) visualState = Il2Cpp.FireState.FullBurn;
                    __instance.m_FX.TriggerStage(visualState, true, true); 
                    iFire.changeFX = false;
                }
                if (iFire.fireSim.state == Enums.FireState.Dwindling && iFire.changeFX)
                {
                    __instance.m_FX.TriggerStage(Il2Cpp.FireState.Starting_TinderSmallFlame, true, true);
                    iFire.changeFX = false;
                }
                if (iFire.fireSim.state == Enums.FireState.Coals && iFire.changeFX)
                {
                    __instance.m_FX.TriggerStage(Il2Cpp.FireState.Starting_TinderSmolder, true, true); //vanilla embers stage used to depict coals
                    iFire.changeFX = false;
                }
                if(iFire.fireSim.state == Enums.FireState.Embers && iFire.changeFX)
                {
                    __instance.m_FX.TriggerStage(Il2Cpp.FireState.Starting_TinderSmolder, true, true); //vanilla starter use stage used to depict embers EXPERIMENTAL
                    iFire.changeFX = false;}
                if (iFire.fireSim.state == Enums.FireState.None)
                {
                    Main.Logger.Log("Fire is out!", ComplexLogger.FlaggedLoggingLevel.Debug);
                    __instance.TurnOff();
                    if (__instance.m_Campfire)
                    {
                        __instance.m_Campfire.SetState(CampfireState.BurntOut);
                    }
                }

            }
        }

        [HarmonyPatch(typeof(Fire), nameof(Fire.AddFuel))]

        public class AddFuelToPool
        {

            public static bool Prefix() => false;

            public static void Postfix(Fire __instance, ref GearItem fuel)
            {

                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                FireFuelItem fuelItem = Utilities.FuelItems.GetFuelItemInstance(fuel);

                iFire.fireSim.AddFuelItem(fuelItem);

                __instance.m_HeatSource.m_TempIncrease = iFire.fireSim.totalRadiantTemperature;
                //__instance.m_FuelHeatIncrease = iFire.totalExternalFireTemperature;
                __instance.m_HeatSource.m_MaxTempIncreaseInnerRadius = iFire.fireSim.innerRadius; 
                __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius = iFire.fireSim.outerRadius;

                if (fuelItem.type == FuelType.Wood || fuelItem.type == FuelType.Synthetic)
                {
                    __instance.m_FX.TriggerFlareupLarge();
                }

                iFire.RefreshDurationEstimate();
            }
        }

       [HarmonyPatch(typeof(Fire), nameof(Fire.PlayerBeginCreate))]

        public class SetFireStarting
        {

            public static void Postfix(Fire __instance)
            {
                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                Main.Logger.Log("Fire is starting!", ComplexLogger.FlaggedLoggingLevel.Debug);
                iFire.fireSim.state = Enums.FireState.Starting;
            }

        }

        [HarmonyPatch(typeof(Fire), nameof(Fire.ExitFireStarting))]

        public class FireStartOverride //this ensures that the fuel item is added ONLY when the fire start is successful
        {

            public static void Postfix(Fire __instance, ref bool success)
            {

                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                if(iFire == null) return;

                Main.Logger.Log($"Exit fire starting, fire success is {success}", ComplexLogger.FlaggedLoggingLevel.Debug);

                if (success)
                {
                    GearItem fi = __instance.m_FuelUsedToStart.gameObject.GetComponent<GearItem>();
                    if (fi == null)
                    {
                        Main.Logger.Log("Fuel source item is null!", ComplexLogger.FlaggedLoggingLevel.Error);
                        return;
                    }

                    Main.Logger.Log($"Started fire using {fi.name}", ComplexLogger.FlaggedLoggingLevel.Debug);

                    FireFuelItem fuelItem = Utilities.FuelItems.GetFuelItemInstance(fi);

                    if(fuelItem == null)
                    {
                        Main.Logger.Log($"Cannot find fuel item profile using {fi.name}", ComplexLogger.FlaggedLoggingLevel.Error);
                        return;
                    }

                    fuelItem.state = FireFuelItemState.Burning;

                    //find a way to add tinder here

                    iFire.fireSim.AddFuelItem(fuelItem);
                    iFire.fireSim.state = Enums.FireState.Dwindling; 

                    iFire.RefreshDurationEstimate();
                }
                else
                {
                    if(__instance.m_FuelUsedToStart != null) GameManager.GetPlayerManagerComponent().ConsumeUnitFromInventory(__instance.m_FuelUsedToStart.gameObject);
                    Main.Logger.Log("Failed to start fire.", ComplexLogger.FlaggedLoggingLevel.Debug);
                    iFire.fireSim.state = Enums.FireState.None;
                }
            }

        }

        [HarmonyPatch(typeof(Fire), nameof(Fire.TurnOn))]

        public class FireTurnOnOverride
        {

            public static bool Prefix() => false;

            public static void Postfix(Fire __instance, ref FuelSourceItem fuel)
            {

                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later
                __instance.m_HeatSource.TurnOn();
                __instance.m_HeatSource.m_MaxTempIncrease = iFire.fireSim.totalRadiantTemperature;
                __instance.m_FuelHeatIncrease = iFire.fireSim.totalRadiantTemperature;
                __instance.m_HeatSource.m_MaxTempIncreaseInnerRadius = iFire.fireSim.innerRadius;
                __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius = iFire.fireSim.outerRadius * __instance.GetFireOuterRadiusScale(); //maybe remove this

                __instance.m_FX.TriggerStage(Il2Cpp.FireState.Starting_TinderSmallFlame, true, true);

                __instance.m_ElapsedOnTODSeconds = 0f;
                __instance.m_ElapsedOnTODSecondsUnmodified = 0f;

            }

        }

        [HarmonyPatch(typeof(Fire), nameof(Fire.TurnOff))]

        public class FireTurnOffOverride
        {

            public static bool Prefix() => false;

            public static void Postfix(Fire __instance)
            {

                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                if(iFire == null) return;

                __instance.StopFireLoop();
                __instance.m_HeatSource.TurnOff(); //maybe for now
                if (__instance.m_FX)
                {
                    __instance.m_FX.StopAllFX();
                }

                //charcoal
                __instance.m_NumGeneratedCharcoalPieces = iFire.fireSim.GetCharcoal(); 
                __instance.FireStateSet(Il2Cpp.FireState.Off);
            }

        }

        [HarmonyPatch(typeof(Fire), nameof(Fire.CanTakeTorch))]

        public class TakeTorchLogic
        {

            public static bool Prefix() => false;

            public static void Postfix(Fire __instance, ref bool __result)
            {
                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                __result = iFire.fireSim.CanTakeTorch();
            }
        }

        [HarmonyPatch(typeof(Fire), nameof(Fire.IsBurning))]
        internal static class Fire_IsBurning
        {
            public static void Postfix(Fire __instance, ref bool __result)
            {

                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                if (__instance.m_IsPerpetual || iFire == null) { return; }
                if (iFire.fireSim.state != Enums.FireState.None)
                {
                    __result = true;
                }
                else __result = false;
            }
        }



        [HarmonyPatch(typeof(Fire), nameof(Fire.GetRemainingLifeTimeSeconds))]

        public class GetEstimatedDurationSeconds
        {
            public static void Postfix(Fire __instance, ref float __result)
            {
                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                if(iFire == null) return;

                if (iFire.fireSim.state > Enums.FireState.Embers)
                {
                    float num = iFire.GetDurationEstimate();
                    __result = num;
                }
                else
                {
                    __result = 0f;
                }


                //Main.Logger.Log($"Remaining lifetime seconds: {__result}", ComplexLogger.FlaggedLoggingLevel.Debug);
            }

        }

        [HarmonyPatch(typeof(Forge), nameof(Forge.ForgeHotEnoughForUse))]

        internal class AdjustForgeUseHeat
        {
            
            private static void Postfix(Forge __instance, ref bool __result)
            {
                
                Fire fire = __instance.m_Fire;
                if(fire == null) return;
                ImprovedFireComponent iFire = fire.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later
                if(iFire == null) return;

                if(iFire.fireSim.forgeTemperature >= FireSimulation.FORGE_TEMPERATURE) __result = true;
                else __result = false;

            }

        }

        [HarmonyPatch(typeof(Forge), nameof(Forge.UpdateInteraction))]

        internal class AddForgeTemperatureHoverText
        {
            
            public static bool Prefix(Forge __instance)
            {
                
                 Fire fire = __instance.m_Fire;
                if(fire == null) return true;
                ImprovedFireComponent iFire = fire.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later
                if(iFire == null) return true;

                return false;
            }
            public static void Postfix(Forge __instance, ref BaseInteraction baseInteraction)
            {
                
                 Fire fire = __instance.m_Fire;
                if(fire == null) return;
                ImprovedFireComponent iFire = fire.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later
                if(iFire == null) return;

                baseInteraction.HoverText = Localization.Get(baseInteraction.m_DefaultHoverText.m_LocalizationID);

                if (iFire.fireSim.forgeTemperature > 0f)
                {
                    string heat = Utils.GetHeatIncreaseText(iFire.fireSim.forgeTemperature, true, __instance.m_Fire);
                    baseInteraction.HoverText = baseInteraction.HoverText + "\n" + heat;
                }

            }

        }

        [HarmonyPatch(typeof(Forge), nameof(Forge.DisplayNotHotEnoughMessage))]

        internal class ChangeTemperatureMessage
        {
            
            public static void Prefix(Forge __instance)
            {
                
                Fire fire = __instance.m_Fire;
                if(fire == null) return;
                ImprovedFireComponent iFire = fire.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later
                if(iFire == null) return;

                __instance.m_MinTemperatureForCrafting = FireSimulation.FORGE_TEMPERATURE;

            }

        }

    }
}
