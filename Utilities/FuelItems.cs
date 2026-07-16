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

        FireFuelItem cedarLog = new FireFuelItem("Cedar", FuelType.Wood, 3600, 120, 300, 4500f, 16_000_00f, 0.7f, 0.9f, 0.15f);
        FireFuelItem firLog = new FireFuelItem("Fir", FuelType.Wood, 4800, 150, 300, 5000f, 24_000_00f, 0.7f, 0.9f, 0.18f);
        FireFuelItem reclaimedWood = new FireFuelItem("Reclaimed Wood", FuelType.Wood, 1800, 90, 325, 3500f, 7_000_00f, 0.65f, 0.88f, 0.1f);
        FireFuelItem stick = new FireFuelItem("Stick", FuelType.Stick, 1080, 20, 200, 1500f, 1_600_00f, 0.55f, 0.85f, 0.08f);
        FireFuelItem coal = new FireFuelItem("Coal", FuelType.Coal, 3000, 300, 500, 3000f, 9_000_00f, 0.2f, 0.95f, 0f);
        FireFuelItem book = new FireFuelItem("Book", FuelType.Paper, 2400, 45, 250, 2500f, 6_000_00f, 0.85f, 0.97f, 0.02f);
        FireFuelItem firelog = new FireFuelItem("Firelog", FuelType.Synthetic, 6600, 180, 350, 5500f, 36_000_00f, 0.75f, 0.95f, 0.05f);


    }
}
