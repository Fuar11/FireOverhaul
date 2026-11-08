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
        public static FireFuelItem GetFuelItemInstance(GearItem gi)
        {

            string name = gi.name;

            if (name.ToLowerInvariant().Contains("softwood")) return new FireFuelItem(gi.name, "Cedar", FuelType.Wood, 3, 650, 4500f, 8_000_000f, 0.7f, 0.9f, 0.15f, gi.GetItemWeightKG().m_Units);
            if (name.ToLowerInvariant().Contains("hardwood")) return new FireFuelItem(gi.name, "Fir", FuelType.Wood, 5f, 720, 6500f, 19_000_000f, 0.7f, 0.9f, 0.18f, gi.GetItemWeightKG().m_Units);
            if (name.ToLowerInvariant().Contains("reclaimed")) return new FireFuelItem(gi.name, "Reclaimed Wood", FuelType.Wood, 1.5f, 550, 3500f, 4_000_000f, 0.65f, 0.88f, 0.1f, gi.GetItemWeightKG().m_Units);
            if (name.ToLowerInvariant().Contains("stick")) return new FireFuelItem(gi.name, "Stick", FuelType.Stick, 2f, 300, 1450f, 1_250_000f, 0.85f, 0.95f, 0.08f, gi.GetItemWeightKG().m_Units);
            if (name.ToLowerInvariant().Contains("torch")) return new FireFuelItem("GEAR_Stick", "Stick", FuelType.Stick, 4f, 300, 1750f, 1_850_000f, 0.85f, 0.95f, 0.08f, 0.1f);
            //if (name.ToLowerInvariant().Contains("charcoal")) return reclaimedWood;
            if (name.ToLowerInvariant().Contains("coal")) return new FireFuelItem(gi.name, "Coal", FuelType.Coal, 8, 800, 6000f, 20_500_000f, 0.2f, 0.95f, 0f, gi.GetItemWeightKG().m_Units);
            if (name.ToLowerInvariant().Contains("book")) return new FireFuelItem(gi.name, "Book", FuelType.Book, 2, 250, 3500f, 6_500_000f, 0.85f, 0.95f, 0.02f, gi.GetItemWeightKG().m_Units);
            if (name.ToLowerInvariant().Contains("firelog")) return new FireFuelItem(gi.name, "Firelog", FuelType.Synthetic, 3f, 350, 5500f, 36_000_000f, 0.75f, 0.95f, 0.05f, gi.GetItemWeightKG().m_Units);

           //tinder/kindling
            if( name.ToLowerInvariant().Contains("gear_tinder") || name.ToLowerInvariant().Contains("bark")) return new FireFuelItem(gi.name, "Wood Tinder", FuelType.Paper, 0.5f, 50, 1500f, 1_000_00f, 0.99f, 0.99f, 0f, gi.GetItemWeightKG().m_Units);
            if (name.ToLowerInvariant().Contains("newsprint") || name.ToLowerInvariant().Contains("paperstack") || name.ToLowerInvariant().Contains("cash")) return new FireFuelItem(gi.name, "Paper Tinder", FuelType.Paper, 0.3f, 50, 2000f, 950_00f, 0.99f, 0.99f, 0f, gi.GetItemWeightKG().m_Units);
            if (name.ToLowerInvariant().Contains("cattail")) return new FireFuelItem(gi.name, "Cattail Head", FuelType.Paper, 0f, 50, 1500f, 350_00f, 0.99f, 0.99f, 0f, gi.GetItemWeightKG().m_Units);
;

            return null;

        }
    }
}
