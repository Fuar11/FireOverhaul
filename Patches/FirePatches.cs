using ImprovedFires.Component;
using ImprovedFires.Enums;
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

            }

        }

        [HarmonyPatch(typeof(Fire), nameof(Fire.Update))]

        public class DisableUpdateLoop
        {
            public static bool Prefix => false;

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
                if (__instance.m_FireState == Il2Cpp.FireState.Off) //completely dead
                {
                    __instance.m_TimeOffSeconds += Time.deltaTime;
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

                if(iFire.state == Enums.FireState.Coals)
                {
                    __instance.m_FX.TriggerStage(Il2Cpp.FireState.Starting_TinderSmolder, true); //vanilla embers stage used to depict coals
                }

                if(iFire.state != Enums.FireState.Embers)
                {
                    __instance.m_FX.TriggerStage(Il2Cpp.FireState.Starting_FireStarterUse, true); //vanilla starter use stage used to depict embers EXPERIMENTAL
                }

                if (iFire.state == Enums.FireState.None)
                {
                    __instance.TurnOff();
                    if (__instance.m_Campfire)
                    {
                        __instance.m_Campfire.SetState(CampfireState.BurntOut);
                    }
                }

            }
        }

        [HarmonyPatch(typeof(Fire), nameof(Fire.TurnOff))]

        public class FireTurnOffOverride
        {

            public static bool Prefix => false;

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
                __instance.m_NumGeneratedCharcoalPieces = iFire.GetCharcoal(); 
                __instance.FireStateSet(Il2Cpp.FireState.Off);
            }

        }

    }
}
