using ImprovedFires.Component;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImprovedFires.Enums;
using ImprovedFires.Simulation;

namespace ImprovedFires.Utilities
{
    public class FuelItems
    {
        public static FireFuelItem GetFuelItemInstance(string name)
        {

            if (name.ToLowerInvariant().Contains("softwood")) return new FireFuelItem("Cedar", FuelType.Wood, 60, 3, 650, 4500f, 8_000_000f, 0.7f, 0.9f, 0.15f);;
            if (name.ToLowerInvariant().Contains("hardwood")) return new FireFuelItem("Fir", FuelType.Wood, 120, 5f, 720, 6500f, 19_000_000f, 0.7f, 0.9f, 0.18f);;
            if (name.ToLowerInvariant().Contains("reclaimed")) return new FireFuelItem("Reclaimed Wood", FuelType.Wood, 58.3f, 1.5f, 550, 3500f, 4_000_000f, 0.65f, 0.88f, 0.1f);;
            if (name.ToLowerInvariant().Contains("stick")) return new FireFuelItem("Stick", FuelType.Stick, 18, 1f, 300, 1750f, 1_850_000f, 0.85f, 0.95f, 0.08f);;
            if (name.ToLowerInvariant().Contains("torch")) return new FireFuelItem("Stick", FuelType.Stick, 18, 1f, 300, 1750f, 1_850_000f, 0.85f, 0.95f, 0.08f);;
            //if (name.ToLowerInvariant().Contains("charcoal")) return reclaimedWood;
            if (name.ToLowerInvariant().Contains("coal")) return new FireFuelItem("Coal", FuelType.Coal, 50, 10, 800, 6000f, 20_500_000f, 0.2f, 0.95f, 0f);;
            if (name.ToLowerInvariant().Contains("book")) return new FireFuelItem("Book", FuelType.Paper, 40, 5, 250, 2500f, 7_500_00f, 0.96f, 0.97f, 0.02f);;
            if (name.ToLowerInvariant().Contains("firelog")) return new FireFuelItem("Firelog", FuelType.Synthetic, 110, 3f, 350, 5500f, 36_000_000f, 0.75f, 0.95f, 0.05f);;

           //tinder/kindling
            if( name.ToLowerInvariant().Contains("gear_tinder") || name.ToLowerInvariant().Contains("bark")) return new FireFuelItem("Wood Tinder", FuelType.Paper, 2, 0.5f, 50, 1500f, 1_000_00f, 0.99f, 0.99f, 0f);;
            if (name.ToLowerInvariant().Contains("newsprint") || name.ToLowerInvariant().Contains("paperstack") || name.ToLowerInvariant().Contains("cash")) return new FireFuelItem("Paper Tinder", FuelType.Paper, 4, 0.3f, 50, 2000f, 950_00f, 0.99f, 0.99f, 0f);;
            if (name.ToLowerInvariant().Contains("cattail")) return new FireFuelItem("Cattail Head", FuelType.Paper, 2, 0f, 50, 1500f, 350_00f, 0.99f, 0.99f, 0f);
;

            return null;

        }


    }
}
