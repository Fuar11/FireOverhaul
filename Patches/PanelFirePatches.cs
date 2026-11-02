using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImprovedFires.Component;

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


        [HarmonyPatch(typeof(Panel_FeedFire), "Update")]
		internal class Panel_FeedFire_Prefix
		{
			private static void Postfix(Panel_FeedFire __instance)
			{
				(__instance.m_Sprite_FireFill as UIBasicSprite).fillAmount = GetFireFillAmount(__instance);
			}

            private static float GetFireFillAmount(Panel_FeedFire __instance)
            {
                ImprovedFireComponent iFire = __instance.m_FireplaceInteraction.Fire.gameObject.GetComponent<ImprovedFireComponent>();

                if (iFire == null)
                {
                    Main.Logger.Log("ImprovedFireComponent is null, cannot get fire fill amount", ComplexLogger.FlaggedLoggingLevel.Error);
                    return 0f;
                }

                switch(iFire.state)
                {
                    case Enums.FireState.Embers:
                        return 0.1f;
                    case Enums.FireState.Coals:
                        return 0.15f;
                    case Enums.FireState.Dwindling:
                        return 0.5f;
                    case Enums.FireState.Steady:
                        return 0.9f;
                    case Enums.FireState.Raging:
                        return 1f;
                    default:
                        return 0f;
                }
                
            }
		}
    }
}
