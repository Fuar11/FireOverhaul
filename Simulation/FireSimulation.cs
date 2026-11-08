using ImprovedFires.Enums;
using FireState = ImprovedFires.Enums.FireState;

namespace ImprovedFires.Simulation
{
    public class FireSimulation
    {

        public FireType type;
        public FireState state;

        public List<FireFuelItem> fuelPool = new List<FireFuelItem>();

        public float ambientTemperature;
        public float totalRadiantTemperature;
        public float totalFireTemperature;
        public float cookingSlotTemperature;
        public float forgeTemperature;
        private float corePower;
        private float totalRadiantPower;
        public float flamePower;
        public float coalsPower;
        public float emberPower;
        public float totalCoalFuelPower; //total power of all coal burning in any state
        public float totalUnweightedPower;
        public float totalFuelEnergy; 
        private int unlitFuelCount; //amount of fuel not currently combusting
        public float burnRateMult; //multiplier for burn rate of all fuel calculated based on the temperature
        public float innerRadius;
        public float outerRadius;

        //airflow
        public float airflowControl = 1f;

        public float airflowMin = 0.25f;

        public float heatConcentrationFactor = 1f;
        public float GetAirflow(float control) => Mathf.Lerp(airflowMin, 1f, control);
        public float airflow => GetAirflow(airflowControl);

        //power
        public const float BASELINE_POWER = 10000f; // watts
        public const float BASELINE_INTERNAL_TEMP = 700f;  // degrees
        public const float FORGE_TEMPERATURE = 1200f;
        public const float MAX_COAL_FIRE_TEMP = 1350f;
        public const float MAX_WOOD_FIRE_TEMP = 1100f;

        public const float FORGE_MIN_ENERGY_HEAT_CONCENTRATION_THRESHOLD = 5_000_000f; //joules 
        public const float FORGE_MAX_ENERGY_HEAT_CONCENTRATION_THRESHOLD = 30_000_000f; //joules

        //temperature change constants
        public const float TEMP_HEAT_TIME_CONSTANT = 30f;  // game seconds
        public const float TEMP_COOL_TIME_CONSTANT = 240f;

        public const float FORGE_HEAT_TIME_CONSTANT = 120f; //for balance, heating up the forge doesn't take too long
        public const float FORGE_COOL_TIME_CONSTANT = 1000f; //but cooling down takes much longer

        public const float EXT_HEAT_TIME_CONSTANT = 60f;   // game seconds
        public const float EXT_COOL_TIME_CONSTANT = 300f;

        public const float COOK_HEAT_TIME_CONSTANT = 120f; // game seconds
        public const float COOK_COOL_TIME_CONSTANT = 480f;

        //output heat
        public const float MAX_RADIANT_HEAT = 80f;     // °C, vanilla max
        public const float RADIANCE_POWER_SCALE = 3000f; // watts of radiant power

        //state controls, in watts

        public const float RAGING_ENTER = 9000f, RAGING_EXIT = 7500f;
        public const float STEADY_ENTER = 2500f, STEADY_EXIT = 2000f;
        public const float DWINDLE_ENTER = 300f, DWINDLE_EXIT = 200f;
        public const float COALS_ENTER_EXIT = 150f;

        public const float HOT_CORE_MIN = 200f; //minimum temperature the fire can be before going out completely if no other fuel is combusting/embers and unburnt fuel is still on the fire
        public const float COOKING_HEAT_TRANSFER_FACTOR = 0.4f; //fraction of the core internal temperature that transitions into the cooking slots
        public const float FORGE_HEAT_TRANSFER_FACTOR = 0.95f; //fraction of core internal temperature that transitions into the forge
        public FireSimulation(FireType type)
        {
            this.type = type;
            SetAirflowMinimum();
            SetHeatConcentration();
        }

        //step

        public void Step(float dt)
        {

            corePower = 0;
            totalRadiantPower = 0;
            totalUnweightedPower = 0;
            totalCoalFuelPower = 0;

            totalFuelEnergy = 0;

            flamePower = 0;
            coalsPower = 0;
            emberPower = 0;
            unlitFuelCount = 0;

            burnRateMult = Mathf.Clamp(Mathf.Pow(Mathf.Max(0f, totalFireTemperature) / BASELINE_INTERNAL_TEMP, 0.4f), 0.5f, 1.5f);

            foreach (FireFuelItem fuel in fuelPool)
            {
                fuel.UpdateStats(this, false, dt);

                int coalsWeight = fuel.state == FireFuelItemState.Coals ? fuel.coalsWeightFactor : 1;
                float weightedAmount = fuel.currentPowerOutput * coalsWeight;

                totalUnweightedPower += fuel.currentPowerOutput;
                corePower += weightedAmount - fuel.heatDrainPower;
                totalRadiantPower += fuel.GetRadiantPower();

                if (fuel.type == FuelType.Coal) totalCoalFuelPower += fuel.currentPowerOutput;

                switch (fuel.state)
                {
                    case Enums.FireFuelItemState.Burning:
                        flamePower += fuel.currentPowerOutput;
                        totalFuelEnergy += fuel.remainingEnergy;
                        break;
                    case Enums.FireFuelItemState.Coals:
                        coalsPower += fuel.currentPowerOutput;
                        totalFuelEnergy += fuel.remainingEnergy;
                        break;
                    case Enums.FireFuelItemState.Embers:
                        emberPower += fuel.currentPowerOutput;
                        totalFuelEnergy += fuel.remainingEnergy;
                        break;
                    case Enums.FireFuelItemState.Heating:
                    case Enums.FireFuelItemState.Cold:
                        unlitFuelCount++;
                        break;
                }
            }

            corePower = Mathf.Max(corePower, 0f); //power can't be negative

            fuelPool.RemoveAll(f => f.state == Enums.FireFuelItemState.Ash);

            //core internal temperature
            float targetTemp = CalculateCoreTemperature();

            float timeConstant = targetTemp > totalFireTemperature
                ? TEMP_HEAT_TIME_CONSTANT
                : TEMP_COOL_TIME_CONSTANT;

            totalFireTemperature = CalculateTemperatureSmoothing(totalFireTemperature, targetTemp, timeConstant, dt);

            //cooking slot temperature
            float targetCookingTemp = Mathf.Lerp(ambientTemperature, totalFireTemperature, COOKING_HEAT_TRANSFER_FACTOR);

            float cookTimeConstant = targetCookingTemp > cookingSlotTemperature
               ? COOK_HEAT_TIME_CONSTANT
               : COOK_COOL_TIME_CONSTANT;

            cookingSlotTemperature = CalculateTemperatureSmoothing(cookingSlotTemperature, targetCookingTemp, cookTimeConstant, dt);

            //forge temperature
            float targetForgeTemp = Mathf.Lerp(ambientTemperature, totalFireTemperature, FORGE_HEAT_TRANSFER_FACTOR);

            float forgeTimeConstant = targetForgeTemp > forgeTemperature ? FORGE_HEAT_TIME_CONSTANT : FORGE_COOL_TIME_CONSTANT;

            forgeTemperature = CalculateTemperatureSmoothing(forgeTemperature, targetForgeTemp, forgeTimeConstant, dt);

            //radiant temperature
            float targetTempExt = CalculateRadiantTemperature();

            float extTimeConstant = targetTempExt > totalRadiantTemperature
               ? EXT_HEAT_TIME_CONSTANT
               : EXT_COOL_TIME_CONSTANT;

            totalRadiantTemperature = CalculateTemperatureSmoothing(totalRadiantTemperature, targetTempExt, extTimeConstant, dt);

            SetInnerAndOuterRadius();

            if (state != FireState.Starting && state != FireState.None)
            {
                state = SetState();
            }

            //Main.Logger.Log($"core {totalFireTemperature:0} | power {corePower:0} | coalShare {coalShareDebug:0.00} | limit {ambientTemperature + riselimitDebug:0}", ComplexLogger.FlaggedLoggingLevel.Debug);
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

            if (coalsPower >= COALS_ENTER_EXIT)
                return FireState.Coals;

            if (coalsPower + emberPower > 0f)
                return FireState.Embers;

            if (unlitFuelCount > 0 && totalFireTemperature >= HOT_CORE_MIN)
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
        private float CalculateCoreTemperature()
        {
            float coalFactor = totalUnweightedPower > 0f ? totalCoalFuelPower / totalUnweightedPower : 0f;
            //coalShareDebug = coalFactor;
            float temperatureGainLimit = Mathf.Lerp(MAX_WOOD_FIRE_TEMP, MAX_COAL_FIRE_TEMP, coalFactor);
            //riselimitDebug = riseLimit;

            float heatConcentrationAmt = type == FireType.Forge ? GetForgeHeatConcentration() : heatConcentrationFactor;

            float temperatureGain = BASELINE_INTERNAL_TEMP * Mathf.Pow(corePower * heatConcentrationAmt / BASELINE_POWER, 0.35f);
            return ambientTemperature + Mathf.Min(temperatureGain, temperatureGainLimit);
        }

        public float CalculateRadiantTemperature()
        {
            return MAX_RADIANT_HEAT * (1f - Mathf.Exp(-totalRadiantPower / RADIANCE_POWER_SCALE));
        }

        private float CalculateTemperatureSmoothing(float mainTemp, float targetTemp, float timeConstant, float dt)
        {
            float k = 1f - Mathf.Exp(-dt / timeConstant);

            return Mathf.Lerp(mainTemp, targetTemp, k);
        }

        private void SetInnerAndOuterRadius()
        {
            float heatScale = totalRadiantTemperature / MAX_RADIANT_HEAT;
            innerRadius = Mathf.Lerp(0.5f, 3f, heatScale);
            outerRadius = Mathf.Lerp(2f, 8f, heatScale);
        }

        //cooking

        public float CalculateCookSpeed(CookType type)
        {

            float min; //minimum cooking slot temperature for given type to cook
            float avg = 260f; //temperature for given food to cook at average rate
            float cap = 450f; //temperature for given food to cook at maximum rate

            float k;
            float rate;

            float minRateAmt = 0.25f;
            float avgRateAmt = 1f;
            float capRateAmt = 2f;

            min = GetCookMinTemp(type);

            if (type == CookType.Snow) //is snow
            {
                minRateAmt = 0.0104f; //at minimum temp it takes 2 full days for 1L of snow to melt
                avgRateAmt = 2f; //at average temp it takes 15 minutes to melt
                capRateAmt = 5f; //at cap temp it takes 5 minutes to melt
            }
            else if (type == CookType.Water) //is water boiling
            {
                avgRateAmt = 2f;
                capRateAmt = 3f;
            }


            if (cookingSlotTemperature > avg)
            {
                k = Mathf.InverseLerp(avg, cap, cookingSlotTemperature);
                rate = Mathf.Lerp(avgRateAmt, capRateAmt, k);
            }
            else
            {
                k = Mathf.InverseLerp(min, avg, cookingSlotTemperature);
                rate = Mathf.Lerp(minRateAmt, avgRateAmt, k);
            }

            return rate;
        }

        public static float GetCookMinTemp(CookType type)
        {
            if (type == CookType.Snow || type == CookType.None) //is snow or can add snow
            {
                return 1f;

            }
            else if (type == CookType.Water) //is water boiling
            {
                return 115f;
            }
            else return 100f;
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

        public void UpdateAirflow(float value)
        {
            airflowControl = value;
        }

        private void SetAirflowMinimum()
        {
            if (type == FireType.Campfire) airflowMin = 0.7f;
            else if (type == FireType.FireBarrel || type == FireType.Fireplace || type == FireType.RimGrill) airflowMin = 0.5f;
            else if (type == FireType.WoodStove || type == FireType.Forge) airflowMin = 0.25f;
            else airflowMin = 0.35f;
        }

        private void SetHeatConcentration()
        {

            if (type == FireType.Campfire) heatConcentrationFactor = 1f; //1 for one
            else if (type == FireType.FireBarrel || type == FireType.Fireplace || type == FireType.RimGrill) heatConcentrationFactor = 1f;
            else if (type == FireType.WoodStove) heatConcentrationFactor = 1.2f;
            else if (type == FireType.Forge) heatConcentrationFactor = 6f; //maximum heat concentration factor for the forge
            else heatConcentrationFactor = 1f;

        }

        private float GetForgeHeatConcentration()
        {
            float p = Mathf.InverseLerp(FORGE_MIN_ENERGY_HEAT_CONCENTRATION_THRESHOLD, FORGE_MAX_ENERGY_HEAT_CONCENTRATION_THRESHOLD, totalFuelEnergy);

            return Mathf.Lerp(1f, heatConcentrationFactor, p);
        }

        public bool CanTakeTorch()
        {
            foreach (FireFuelItem fuel in fuelPool)
            {
                if (fuel.type == Enums.FuelType.Stick || fuel.name == "Reclaimed Wood" && fuel.burnAmount <= 0.3f)
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
                if (fuel.type == Enums.FuelType.Stick || fuel.name == "Reclaimed Wood" && fuel.burnAmount <= 0.3f)
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

            foreach (FireFuelItem fuel in fuelPool)
            {
                clonedList.Add(fuel.Clone());
            }

            clone.fuelPool = clonedList;

            return clone;
        }

    }
}