using ImprovedFires.Enums;
using FireState = ImprovedFires.Enums.FireState;

namespace ImprovedFires.Simulation
{
    public class FireSimulation
    {

        public FireState state;

        public List<FireFuelItem> fuelPool = new List<FireFuelItem>();

        public float totalExternalFireTemperature;
        public float totalFireTemperature;
        private float corePower;
        private float totalRadiantPower;
        public float flamePower;
        public float coalsPower;
        public float emberPower;

        private int unlitFuelCount; //amount of fuel not currently combusting

        public float burnRateMult; //multiplier for burn rate of all fuel calculated based on the temperature

        public float innerRadius;
        public float outerRadius;

        //power
        public const float BASELINE_POWER = 10000f; // watts
        public const float BASELINE_INTERNAL_TEMP = 700f;  // degrees

        //temperature control
        public const float TEMP_HEAT_TIME_CONSTANT = 30f;  // game seconds
        public const float TEMP_COOL_TIME_CONSTANT = 240f; // game seconds

        public const float EXT_HEAT_TIME_CONSTANT = 60f;   // game seconds
        public const float EXT_COOL_TIME_CONSTANT = 300f;  // game seconds

        public const float MAX_RADIANT_HEAT = 80f;     // °C, vanilla max
        public const float RADIANCE_POWER_SCALE = 3000f; // watts of radiant power

        //state controls, in watts

        public const float RAGING_ENTER = 9000f, RAGING_EXIT = 7500f;
        public const float STEADY_ENTER = 2500f, STEADY_EXIT = 2000f;
        public const float DWINDLE_ENTER = 300f, DWINDLE_EXIT = 200f;
        public const float COALS_MIN = 150f;

        public const float HOT_CORE_MIN = 200f; //minimum temperature the fire can be before going out completely if no other fuel is combusting/embers and unburnt fuel is still on the fire

        //step

        public void Step(float dt)
        {
            
            corePower = 0;
            totalRadiantPower = 0;

            flamePower = 0;
            coalsPower = 0;
            emberPower = 0;
            unlitFuelCount = 0;

            burnRateMult = Mathf.Clamp(Mathf.Pow(totalFireTemperature / BASELINE_INTERNAL_TEMP, 0.4f), 0.5f, 1.5f);

            foreach (FireFuelItem fuel in fuelPool)
            {
                fuel.UpdateStats(this, false, dt);

                int coalsWeight = fuel.state == FireFuelItemState.Coals ? fuel.coalsWeightFactor : 1;

                corePower += (fuel.currentPowerOutput * coalsWeight) - fuel.heatDrainPower;
                totalRadiantPower += fuel.GetRadiantPower();

                switch (fuel.state)
                {
                    case Enums.FireFuelItemState.Burning:
                        flamePower += fuel.currentPowerOutput;
                        break;
                    case Enums.FireFuelItemState.Coals:
                        coalsPower += fuel.currentPowerOutput;
                        break;
                    case Enums.FireFuelItemState.Embers:
                        emberPower += fuel.currentPowerOutput;
                        break;
                    case Enums.FireFuelItemState.Heating:
                    case Enums.FireFuelItemState.Cold:
                        unlitFuelCount++;
                        break;
                }
            }

            corePower = Mathf.Max(corePower, 0f);

            fuelPool.RemoveAll(f => f.state == Enums.FireFuelItemState.Ash);

            float targetTemp = CalculateTemperature();

            float timeConstant = targetTemp > totalFireTemperature
                ? TEMP_HEAT_TIME_CONSTANT
                : TEMP_COOL_TIME_CONSTANT;

            float k = 1f - Mathf.Exp(-dt / timeConstant);
            totalFireTemperature = Mathf.Lerp(totalFireTemperature, targetTemp, k);

            float targetTempExt = CalculateExternalTargetTemperature();

            float externalTimeConstant = targetTempExt > totalExternalFireTemperature
                ? EXT_HEAT_TIME_CONSTANT
                : EXT_COOL_TIME_CONSTANT;

            float extK = 1f - Mathf.Exp(-dt / externalTimeConstant);

            totalExternalFireTemperature = Mathf.Lerp(totalExternalFireTemperature, targetTempExt, extK);

            SetInnerAndOuterRadius();

            if(state != FireState.Starting && state != FireState.None)
            {
                 state = SetState();
                //do more effects based on temp
            }

        }

        //state

        private FireState SetState()
        {

            FireState fireState = FireState.None;

            bool wasRaging = state == FireState.Raging;
            bool wasSteadyOrGreater = state == FireState.Steady || wasRaging;
            bool wasFlaming = wasSteadyOrGreater || state == FireState.Dwindling;

            

            if (flamePower >= RAGING_ENTER || (wasRaging && flamePower >= RAGING_EXIT))
                return FireState.Raging;

            if (flamePower >= STEADY_ENTER || (wasSteadyOrGreater && flamePower >= STEADY_EXIT))
                return FireState.Steady;

            if (flamePower >= DWINDLE_ENTER || (wasFlaming && flamePower >= DWINDLE_EXIT))
                return FireState.Dwindling;

            if (coalsPower >= COALS_MIN)
                return FireState.Coals;

            if (coalsPower + emberPower > 0f)
                return FireState.Embers;

            if( unlitFuelCount > 0 && totalFireTemperature >= HOT_CORE_MIN)
            {
                return FireState.Embers;
            }

            return fireState;
        }


        //fuel methods
        public void AddFuelItem(FireFuelItem item)
        {
            Main.Logger.Log($"Adding fuel item {item.name}", ComplexLogger.FlaggedLoggingLevel.Debug);
            fuelPool.Add(item);
        }

        public void RemoveFuelItem(FireFuelItem item)
        {
            //Main.Logger.Log($"Removing fuel item {item.name}", ComplexLogger.FlaggedLoggingLevel.Debug);
            fuelPool.Remove(item);
        }

        //temperature methods
        private float CalculateTemperature()
        {
            if (corePower <= 0)
                return 20f; // ambient, maybe modify this by the air temp

            float ratio = corePower / BASELINE_POWER;

            float temp = BASELINE_INTERNAL_TEMP * Mathf.Pow(ratio, 0.35f);

            return Mathf.Clamp(temp, 20f, 1200f);
        }

        public float CalculateExternalTargetTemperature()
        {
            return MAX_RADIANT_HEAT * (1f - Mathf.Exp(-totalRadiantPower / RADIANCE_POWER_SCALE));
        }

        private void SetInnerAndOuterRadius()
        {
            float heatScale = totalExternalFireTemperature / MAX_RADIANT_HEAT;
            innerRadius = Mathf.Lerp(0.5f, 3f, heatScale);
            outerRadius = Mathf.Lerp(2f, 8f, heatScale);
        }

        //other methods

        public void BlowOnFire()
        {
            if (totalFireTemperature <= 200f) //these are testing values
            {
                totalFireTemperature *= 4f;
            }
            else totalFireTemperature /= 20f;
        }

        public bool CanTakeTorch()
        {
            foreach (FireFuelItem fuel in fuelPool)
            {
                if(fuel.type == Enums.FuelType.Stick || fuel.name == "Reclaimed Wood" && fuel.burnAmount <= 0.3f)
                {
                    return true;
                }
            }

            return false;
        }

        public void TakeTorch()
        {
            Main.Logger.Log("Attempting to take torch.", ComplexLogger.FlaggedLoggingLevel.Debug);

            foreach (FireFuelItem fuel in fuelPool)
            {
                if(fuel.type == Enums.FuelType.Stick || fuel.name == "Reclaimed Wood" && fuel.burnAmount <= 0.3f)
                {
                    RemoveFuelItem(fuel);
                    return;
                }
            }
        }

        public int GetCharcoal()
        {

            float charcoalAmount = 0f;

            foreach (FireFuelItem fuel in fuelPool)
            {
                charcoalAmount += fuel.GetCharcoalProduced();
            }

            return Mathf.CeilToInt(charcoalAmount / 0.1f);

        }

        public FireSimulation Clone()
        {
            FireSimulation clone = (FireSimulation)this.MemberwiseClone();
    
            List<FireFuelItem> clonedList = new List<FireFuelItem>();

            foreach(FireFuelItem fuel in fuelPool)
            {
                clonedList.Add(fuel.Clone());
            } 

            clone.fuelPool = clonedList;

            return clone;
        }

    }
}