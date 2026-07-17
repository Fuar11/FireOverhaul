using ImprovedFires.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImprovedFires.Enums;

namespace ImprovedFires.Utilities
{
    public class FuelItems
    {

        static FireFuelItem cedarLog = new FireFuelItem("Cedar", FuelType.Wood, 3600, 120, 300, 4500f, 8_000_00f, 0.7f, 0.9f, 0.15f);
        static FireFuelItem firLog = new FireFuelItem("Fir", FuelType.Wood, 7200, 150, 300, 6500f, 19_000_00f, 0.7f, 0.9f, 0.18f);
        static FireFuelItem reclaimedWood = new FireFuelItem("Reclaimed Wood", FuelType.Wood, 3500, 90, 325, 3500f, 4_000_00f, 0.65f, 0.88f, 0.1f);
        static FireFuelItem stick = new FireFuelItem("Stick", FuelType.Stick, 1080, 20, 200, 3000f, 1_850_00f, 0.85f, 0.95f, 0.08f);
        static FireFuelItem coal = new FireFuelItem("Coal", FuelType.Coal, 3000, 300, 500, 3500f, 7_500_00f, 0.2f, 0.95f, 0f);
        static FireFuelItem book = new FireFuelItem("Book", FuelType.Paper, 2400, 45, 250, 2500f, 7_500_00f, 0.9f, 0.97f, 0.02f);
        static FireFuelItem firelog = new FireFuelItem("Firelog", FuelType.Synthetic, 6600, 180, 350, 5500f, 36_000_00f, 0.75f, 0.95f, 0.05f);

        public static FireFuelItem GetFuelItemInstance(string name)
        {

            if (name.ToLowerInvariant().Contains("cedar")) return cedarLog;
            if (name.ToLowerInvariant().Contains("fir")) return firLog;
            if (name.ToLowerInvariant().Contains("reclaimed")) return reclaimedWood;
            if (name.ToLowerInvariant().Contains("stick")) return stick;
            if (name.ToLowerInvariant().Contains("torch")) return stick;
            //if (name.ToLowerInvariant().Contains("charcoal")) return reclaimedWood;
            if (name.ToLowerInvariant().Contains("coal")) return coal;
            if (name.ToLowerInvariant().Contains("book")) return book;
            if (name.ToLowerInvariant().Contains("firelog")) return firelog;

            return null;

        }


    }
}
