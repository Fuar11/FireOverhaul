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
                    Main.Logger.Log("Adding fire component to fire object, wahoo!", ComplexLogger.FlaggedLoggingLevel.Debug);
                    __instance.gameObject.AddComponent<ImprovedFireComponent>();
                }

                GameManager.GetFireManagerComponent().m_MaxDurationHoursOfFire = float.PositiveInfinity;


            }

        }

        [HarmonyPatch(typeof(Fire), nameof(Fire.Update))]

        public class DisableUpdateLoop
        {
            public static bool Prefix() => false;

            public static void Postfix(Fire __instance)
            {

                if (GameManager.m_IsPaused || GameManager.s_IsGameplaySuspended)
                {
                    return;
                }

                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

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
                __instance.UpdateFireAudio();

                //NEEDS FIXING. FIRES DONT GIVE ANY LIGHT CURRENTLY

                if (iFire.fireSim.state == Enums.FireState.Raging && iFire.changeFX)
                {
                    __instance.m_FX.TriggerStage(Il2Cpp.FireState.FullBurn, true, true); //need something else here
                    iFire.changeFX = false;
                }
                if (iFire.fireSim.state == Enums.FireState.Steady && iFire.changeFX)
                {
                    __instance.m_FX.TriggerStage(Il2Cpp.FireState.FullBurn, true, true); 
                    iFire.changeFX = false;
                }
                if (iFire.fireSim.state == Enums.FireState.Dwindling && iFire.changeFX)
                {
                    __instance.m_FX.TriggerStage(Il2Cpp.FireState.Starting_MediumBurn, true, true);
                    iFire.changeFX = false;
                }
                if (iFire.fireSim.state == Enums.FireState.Coals && iFire.changeFX)
                {
                    __instance.m_FX.TriggerStage(Il2Cpp.FireState.Starting_TinderSmallFlame, true, true); //vanilla embers stage used to depict coals
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

                FireFuelItem fuelItem = Utilities.FuelItems.GetFuelItemInstance(fuel.name);
                fuelItem.gearItemRef = fuel;

                iFire.fireSim.AddFuelItem(fuelItem);

                __instance.m_HeatSource.m_TempIncrease = iFire.fireSim.totalExternalFireTemperature;
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

                if (success)
                {
                    GearItem fi = __instance.m_FuelUsedToStart.gameObject.GetComponent<GearItem>();
                    if (fi == null)
                    {
                        Main.Logger.Log("Fuel source item is null!", ComplexLogger.FlaggedLoggingLevel.Error);
                        return;
                    }
                    FireFuelItem fuelItem = Utilities.FuelItems.GetFuelItemInstance(fi.name);
                    fuelItem.gearItemRef = fi;
                    fuelItem.burnAmount += 0.01f; //a little push
                    fuelItem.state = FireFuelItemState.Burning;

                    //find a way to add tinder here

                    iFire.fireSim.AddFuelItem(fuelItem);
                    iFire.fireSim.state = Enums.FireState.Dwindling; 

                    iFire.RefreshDurationEstimate();
                }
                else
                {
                    Main.Logger.Log("Failed to start fire, setting state back to none.", ComplexLogger.FlaggedLoggingLevel.Debug);
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
                __instance.m_HeatSource.m_MaxTempIncrease = iFire.fireSim.totalExternalFireTemperature;
                __instance.m_FuelHeatIncrease = iFire.fireSim.totalExternalFireTemperature;
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

        [HarmonyPatch(typeof(Fire), nameof(Fire.GetRemainingLifeTimeSeconds))]

        public class GetEstimatedDurationSeconds
        {
            public static void Postfix(Fire __instance, ref float __result)
            {
                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                if (iFire.fireSim.state > Enums.FireState.Embers)
                {
                    float num = iFire.GetDurationEstimate();
                    __instance.m_ElapsedOnTODSeconds = num;
                    __result = num;
                }
                else
                {
                    __result = 0f;
                }


                //Main.Logger.Log($"Remaining lifetime seconds: {__result}", ComplexLogger.FlaggedLoggingLevel.Debug);
            }

        }
    }
}
