using ImprovedFires.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedFires.Patches
{
    internal class UIPatches
    {

        [HarmonyPatch(typeof(FireplaceInteraction), nameof(FireplaceInteraction.GetHoverText))]

        public class HoverTextOverride
        {

            public static void Postfix(FireplaceInteraction __instance, ref string __result)
            {

                ImprovedFireComponent iFire = __instance.gameObject.GetComponent<ImprovedFireComponent>(); //for now, might use GUID later

                Fire fire = __instance.Fire;
                string extraText = "";

                if (fire == null || iFire == null)
                {
                    __result = null;
                    return;
                }

                if(fire.m_FireState != FireState.Off)
                {

                    string state = "";

                    if (iFire.state == Enums.FireState.Embers) state = "Embers";
                    else if (iFire.state == Enums.FireState.Coals) state = "Glowing Coals";
                    else if (iFire.state == Enums.FireState.Dwindling) state = "Dwindling";
                    else if (iFire.state == Enums.FireState.Steady) state = "Steady Burn";
                    else if (iFire.state == Enums.FireState.Raging) state = "Ragin";
                    else state = "Unknown";

                    string internalTemp = iFire.totalFireTemperature.ToString();
                    string externalTemp = iFire.totalExternalFireTemperature.ToString();

                    string[] values = new string[4];

                    values[0] = " (";

                    values[1] = state;

                    values[2] = "\n";

                    values[3] = fire.GetHeatIncreaseText(
                        true
                    );

                    values[4] = " - ";

                    values[5] = internalTemp;

                    extraText = string.Concat(values);

                }

                LocalizedString defaultText = __instance.m_DefaultHoverText;

                if (defaultText != null)
                {
                    string localized = Localization.Get(defaultText.m_LocalizationID);

                    __result = string.Concat(localized, extraText);
                }

                __result = extraText;

            }

        }
    }
}
