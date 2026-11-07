using ImprovedFires.Enums;
using FireState = ImprovedFires.Enums.FireState;

namespace ImprovedFires.Simulation
{
    public class FireSimulation
    {

        public FireType type;
        public FireState state;

        public List<FireFuelItem> fuelPool = new List<FireFuelItem>();

        public float totalExternalFireTemperature;
        public float totalFireTemperature;

        public float cookingSlotTemperature;
        private float corePower;
        private float totalRadiantPower;
        public float flamePower;
        public float coalsPower;
        public float emberPower;

        private int unlitFuelCount; //amount of fuel not currently combusting

        public float burnRateMult; //multiplier for burn rate of all fuel calculated based on the temperature

        public float innerRadius;
        public float outerRadius;

        //airflow
        public float airflowControl = 1f;

        public float airflowMin = 0.25f;
        public float GetAirflow(float control) => Mathf.Lerp(airflowMin, 1f, control); 
        public float airflow => GetAirflow(airflowControl);

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
        public const float COOKING_HEAT_TRANSFER_FACTOR = 0.4f; //fraction of the core internal temperature that transitions into the cooking slots
        public FireSimulation(FireType type)
        {
            
            this.type = type;
            SetAirflowMinimum();

        }

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

            totalFireTemperature = CalculateTemperatureSmoothing(totalFireTemperature, targetTemp, timeConstant, dt);

            float targetCookingTemp = totalFireTemperature * COOKING_HEAT_TRANSFER_FACTOR;

             float cookTimeConstant = targetCookingTemp > cookingSlotTemperature
                ? TEMP_HEAT_TIME_CONSTANT
                : TEMP_COOL_TIME_CONSTANT;

            cookingSlotTemperature = CalculateTemperatureSmoothing(cookingSlotTemperature, targetCookingTemp, cookTimeConstant, dt);

            float targetTempExt = CalculateExternalTargetTemperature();

             float extTimeConstant = targetTempExt > totalExternalFireTemperature
                ? EXT_HEAT_TIME_CONSTANT
                : EXT_COOL_TIME_CONSTANT;

            totalExternalFireTemperature = CalculateTemperatureSmoothing(totalExternalFireTemperature, targetTempExt, extTimeConstant, dt);

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

        private float CalculateTemperatureSmoothing(float mainTemp, float targetTemp, float timeConstant, float dt)
        {
            float k = 1f - Mathf.Exp(-dt / timeConstant);

            return Mathf.Lerp(mainTemp, targetTemp, k);
        }

        private void SetInnerAndOuterRadius()
        {
            float heatScale = totalExternalFireTemperature / MAX_RADIANT_HEAT;
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

            if(type == CookType.Snow) //is snow
            {
                minRateAmt = 0.0104f; //at minimum temp it takes 2 full days for 1L of snow to melt
                avgRateAmt = 2f; //at average temp it takes 15 minutes to melt
                capRateAmt = 5f; //at cap temp it takes 5 minutes to melt
            }
            else if(type == CookType.Water) //is water boiling
            {
                avgRateAmt = 2f;
                capRateAmt = 3f;                
            }


            if(cookingSlotTemperature > avg)
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
            if(type == CookType.Snow) //is snow
            {
                return 1f;
               
            }
            else if(type == CookType.Water) //is water boiling
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
            if(type == FireType.Campfire) airflowMin = 0.7f;
            else if(type == FireType.FireBarrel || type == FireType.Fireplace || type == FireType.RimGrill) airflowMin = 0.5f; 
            else if(type == FireType.WoodStove || type == FireType.Forge) airflowMin = 0.25f;
            else airflowMin = 0.35f;
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