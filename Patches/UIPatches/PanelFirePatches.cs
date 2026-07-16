using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedFires.Patches.UIPatches
{
    internal class PanelFirePatches
    {

        [HarmonyPatch(typeof(Panel_FireStart), nameof(Panel_FireStart.FilterItem))]
        public class RemoveBigFuelFromFireStart
        {
            public static void Postfix(ref bool __result, ref GearItem gi)
            {
                if (GameManager.GetSkillFireStarting().GetCurrentTierNumber() > 4) return;
                if (gi.name.ToLowerInvariant().Contains("hardwood") || gi.name.ToLowerInvariant().Contains("softwood") || gi.name.ToLowerInvariant().Contains("firelog")) __result = false;
            }
        }

    }
}
