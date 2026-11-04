using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImprovedFires.Component;
using ImprovedFires.Simulation;

namespace ImprovedFires.Patches
{
    internal class PanelFirePatches
    {

        [HarmonyPatch(typeof(Panel_FireStart), nameof(Panel_FireStart.FilterItem))]
        public class RemoveBigFuelFromFireStart
        {
            public static void Postfix(ref bool __result, ref GearItem gi)
            {
                if (GameManager.GetSkillFireStarting().GetCurrentTierNumber() > 4) return;
                if (gi.name.ToLowerInvariant().Contains("hardwood") || gi.name.ToLowerInvariant().Contains("softwood") || gi.name.ToLowerInvariant().Contains("firelog") || gi.name.ToLowerInvariant().Contains("reclaimed") || gi.name.ToLowerInvariant().Contains("stick")) __result = false;
            }
        }

        [HarmonyPatch(typeof(Panel_FireStart), nameof(Panel_FireStart.GetSelectedTinder))]

        public class DelegateTinderToFuel
        {
            public static void Postfix(ref FuelSourceItem __result, ref Panel_FireStart __instance)
            {
                __result = __instance.GetSelectedFuelSource();
            }
        }


        [HarmonyPatch(typeof(Panel_FeedFire), nameof(Panel_FeedFire.Update))]
		internal class Panel_FeedFire_Update
		{

            static float fill;

			private static void Postfix(Panel_FeedFire __instance)
			{
                SetFireFillAmount(__instance);
				(__instance.m_Sprite_FireFill as UIBasicSprite).fillAmount = fill;
			}

            private static void SetFireFillAmount(Panel_FeedFire __instance)
            {
                ImprovedFireComponent iFire = __instance.m_FireplaceInteraction.Fire.gameObject.GetComponent<ImprovedFireComponent>();

                if (iFire == null)
                {
                    Main.Logger.Log("ImprovedFireComponent is null, cannot get fire fill amount", ComplexLogger.FlaggedLoggingLevel.Error);
                    return;
                }

                float target;

                switch (iFire.fireSim.state)
                {
                    case Enums.FireState.Embers:    target = Band(0.02f, 0.10f, iFire.fireSim.emberPower,   0f,           300f); break;
                    case Enums.FireState.Coals:     target = Band(0.10f, 0.25f, iFire.fireSim.coalsPower, FireSimulation.COALS_MIN,    2000f); break;
                    case Enums.FireState.Dwindling: target = Band(0.25f, 0.50f, iFire.fireSim.flamePower, FireSimulation.DWINDLE_ENTER, FireSimulation.STEADY_ENTER); break;
                    case Enums.FireState.Steady:    target = Band(0.50f, 0.85f, iFire.fireSim.flamePower, FireSimulation.STEADY_ENTER,  FireSimulation.RAGING_ENTER); break;
                    case Enums.FireState.Raging:    target = Band(0.85f, 1.00f, iFire.fireSim.flamePower, FireSimulation.RAGING_ENTER,  18000f); break;
                    default:   target = 0f; break;
                }
    
                float k = 1f - Mathf.Exp(-Time.unscaledDeltaTime / 0.3f); // ~0.3 real seconds
                fill = Mathf.Lerp(fill, target, k);
            }

            private static float Band(float min, float max, float power, float powerLow, float powerHigh)
            {
                return Mathf.Lerp(min, max, Mathf.InverseLerp(powerLow, powerHigh, power));
            }

		}

        [HarmonyPatch(typeof(Panel_FeedFire), nameof(Panel_FeedFire.RefreshFireDurationLabel))]

        internal class FireDurationLabelTest
        {
            
            public static void Postfix(Panel_FeedFire __instance)
            {
                
                ImprovedFireComponent iFire = __instance.m_FireplaceInteraction.Fire.gameObject.GetComponent<ImprovedFireComponent>();

                float num = iFire.GetDurationEstimate() / 60f;

                if(iFire.fireSim.state == Enums.FireState.Embers)
                {
                    __instance.m_Label_EstimatedFireDuration.text = Localization.Get("GAMEPLAY_Embers");
                    return;
                }

                int durationMinutes = Mathf.FloorToInt(num);

                if (num < 1f)
                {
                    durationMinutes = Mathf.CeilToInt(num);
                }

                string durationString = Il2Cpp.Utils.GetDurationString(durationMinutes);
		        __instance.m_Label_EstimatedFireDuration.text = durationString;
            }

        }


    }
}
