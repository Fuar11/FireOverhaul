using ImprovedFires.Enums;
using FireState = ImprovedFires.Enums.FireState;

namespace ImprovedFires.Simulation
{
    public class FireSimulationSaveDataProxy
    {
        public FireState state { get; set; }
        public float totalFireTemperature { get; set; }
        public float totalRadiantTemperature { get; set; }
        public float cookingSlotTemperature { get; set; }
        public float forgeTemperature { get; set; }
        public float airflowControl { get; set; } = 1f;

        public float timeAtSave {get; set;}

        public List<FireFuelItemSaveDataProxy> fuelPool { get; set; } = new List<FireFuelItemSaveDataProxy>();
    }

    public class FireFuelItemSaveDataProxy
    {
        public string gearName { get; set; } = "";
        public FireFuelItemState state { get; set; }

        public float remainingEnergy { get; set; }
        public float burnAmount { get; set; }
        public float ignitionProgress { get; set; }
        public float preheatEnergyRemaining { get; set; }
    }
}
