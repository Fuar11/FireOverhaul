using ImprovedFires.Component;
using ImprovedFires.Enums;
using ImprovedFires.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ComplexLogger.Utilities.ConversionUtilities;

namespace ImprovedFires.Simulation
{
    public class FireFuelItem
    {
        public GearItem gearItemRef = null;
        public string name = "default fuel item";

        public FireFuelItemState state = FireFuelItemState.None;
        public FuelType type = FuelType.None;

        //definition
        public float energyContent;        // Total energy stored, in joules
        public float remainingEnergy;      // Current energy stored, in joules
        public float maxPowerOutput;       // Maximum wattage output
        public float coalsPowerOutput;     // Maximum wattage output when burning residual carbon
        public float emberPowerOutput;     // Remaining wattage output when emitting lasting ambient heat
        public float currentPowerOutput;   // Current wattage output
        public float burningRadiantFraction; // Portion of energy output into radiant heat felt by the player
        public float coalsRadiantFraction;
        public float embersRadiantFraction;
        public float timeToIgniteAtTemp;   // Time to ignite at ignition temperature in minutes
        public float minIgnitionTemp;      // Minimum temperature at which combustion can commence
        public float ignitionTemp;         // Optimal temperature at which combustion can commence
        public float coalsThreshold;       // Amount of total fuel consumption required for fuel to be burning as coals
        public float emberThreshold;       // Amount of total fuel consumption required for fuel to be burning as embers
        public float charcoalYield;

        //ignition and fueling
        public float preheatEnergyRemaining;
        public float heatDrainPower;
        public float ignitionProgress = 0f;

        //percentages of consumption
        public float burnAmount = 0f; //amount of fuel consumed before turning to coals or embers, 0 = full, 1 = fully consumed
        public float coalsAmount = 0f; //amount of fuel consumed when it is coals (a fraction of the total fuel)
        public float embersAmount = 0f;

        //weighing

        public int coalsWeightFactor;

        public const float HEAT_DRAIN_RATE = 0.2f;
        public const float IGNITION_RANGE_FACTOR = 0.85f;
        public const float MIN_IGNITION_SPEED = 0.25f;

        //airflow 

        public const float AIRFLOW_EXP_BURNING = 1.0f;
        public const float AIRFLOW_EXP_COALS = 1.5f;
        public const float AIRFLOW_EXP_EMBERS = 1.5f;

        public FireFuelItem(string name, FuelType type, float timeToIgniteAtTemp, float ignitionTemp, float maxPowerOutput, float energyContent, float coalsThreshold, float embersThreshold, float charcoalYield)
        {
            this.name = name;
            this.type = type;
            this.timeToIgniteAtTemp = timeToIgniteAtTemp;
            this.ignitionTemp = ignitionTemp;
            this.maxPowerOutput = maxPowerOutput;
            this.energyContent = energyContent;
            this.coalsThreshold = coalsThreshold;
            this.emberThreshold = embersThreshold;
            this.charcoalYield = charcoalYield;

            this.minIgnitionTemp = ignitionTemp * IGNITION_RANGE_FACTOR;

            remainingEnergy = energyContent;
            preheatEnergyRemaining = 0.02f * remainingEnergy;

            float coalsPowerFactor = 0f;
            float emberPowerFactor = 0f;

            if(type == FuelType.Wood)
            {
                coalsPowerFactor = 0.2f;
                emberPowerFactor = 0.095f;
                coalsWeightFactor = 4;
                burningRadiantFraction = 0.25f;
                coalsRadiantFraction = 0.65f;
                embersRadiantFraction = 0.45f;
            }
            else if (type == FuelType.Stick)
            {
                coalsPowerFactor = 0.07f;
                emberPowerFactor = 0.02f;
                coalsWeightFactor = 4;
                burningRadiantFraction = 0.2f;
                coalsRadiantFraction = 0.6f;
                embersRadiantFraction = 0.4f;
            }
            else if(type == FuelType.Coal)
            {
                coalsPowerFactor = 0.85f;
                emberPowerFactor = 0.3f;
                coalsWeightFactor = 1;
                burningRadiantFraction = 0.45f;
                coalsRadiantFraction = 0.75f;
                embersRadiantFraction = 0.5f;
            }
            else if (type == FuelType.Paper)
            {
                coalsPowerFactor = 0.01f;
                emberPowerFactor = 0.001f;
                coalsWeightFactor = 4;
                burningRadiantFraction = 0.15f;
                coalsRadiantFraction = 0.4f;
                embersRadiantFraction = 0.3f;
            }
            else if (type == FuelType.Book)
            {
                coalsPowerFactor = 0.1f;
                emberPowerFactor = 0.04f;
                coalsWeightFactor = 4;
                burningRadiantFraction = 0.2f;
                coalsRadiantFraction = 0.45f;
                embersRadiantFraction = 0.1f;
            }
            else if (type == FuelType.Synthetic)
            {
                coalsPowerFactor = 0.15f;
                emberPowerFactor = 0.095f;
                coalsWeightFactor = 4;
                burningRadiantFraction = 0.3f;
                coalsRadiantFraction = 0.65f;
                embersRadiantFraction = 0.45f;
            }

            coalsPowerOutput = maxPowerOutput * coalsPowerFactor;
            emberPowerOutput = maxPowerOutput * emberPowerFactor;

            state = FireFuelItemState.Cold;

        }

    
        public void UpdateStats(FireSimulation fireSim, bool extinguish, float gameDeltaTime)
        {

            float ignitionRangePosition = Mathf.InverseLerp(minIgnitionTemp, ignitionTemp, fireSim.totalFireTemperature);
            float speed = Mathf.Lerp(MIN_IGNITION_SPEED, 1, ignitionRangePosition);
            float timeToIgniteActual = timeToIgniteAtTemp / speed;

            heatDrainPower = 0f;
            if ((state == FireFuelItemState.Cold || state == FireFuelItemState.Heating) && preheatEnergyRemaining > 0f && gameDeltaTime > 0f)
            {
                float absorbtion = maxPowerOutput * HEAT_DRAIN_RATE * Mathf.Clamp01(fireSim.totalFireTemperature / ignitionTemp);
                heatDrainPower = Mathf.Min(absorbtion, preheatEnergyRemaining / gameDeltaTime);
                preheatEnergyRemaining -= heatDrainPower * gameDeltaTime;
            }

            if (state == FireFuelItemState.Cold)
            {

                ignitionProgress = Mathf.Max(0f, ignitionProgress - gameDeltaTime / (timeToIgniteAtTemp * 60f)); //this ensures if it was previously lit the process isn't restarted completely
                if (fireSim.totalFireTemperature >= minIgnitionTemp)
                {
                    //Main.Logger.Log($"Fire temp is hot enough to ignite fuel {name}", ComplexLogger.FlaggedLoggingLevel.Debug);
                    state = FireFuelItemState.Heating;
                }
            }
            else if (state == FireFuelItemState.Heating)
            {

                if (fireSim.totalFireTemperature < minIgnitionTemp || extinguish) state = FireFuelItemState.Cold;

                float excessTemp = fireSim.totalFireTemperature - ignitionTemp;

                float heatingRatio = Mathf.Clamp(
                    1f + excessTemp / 500f,
                    1f,
                    2f
                );
                float ignitionDuration = (timeToIgniteActual * 60f)  / heatingRatio;

                ignitionProgress += gameDeltaTime / ignitionDuration;
                if (ignitionProgress >= 1f)
                {
                    state = FireFuelItemState.Burning;
                }
            }
            else if (state == FireFuelItemState.Burning)
            {
                currentPowerOutput = maxPowerOutput * CalculateCombustionCurve() * fireSim.burnRateMult;
                currentPowerOutput *= Mathf.Pow(fireSim.airflow, AIRFLOW_EXP_BURNING);

                if (burnAmount >= coalsThreshold) state = FireFuelItemState.Coals;
            }
            else if(state == FireFuelItemState.Coals)
            {
                currentPowerOutput = coalsPowerOutput * CalculateSlowDecayCurve() * fireSim.burnRateMult;
                currentPowerOutput *= Mathf.Pow(fireSim.airflow, AIRFLOW_EXP_COALS);

                if (burnAmount >= emberThreshold) state = FireFuelItemState.Embers;
            }
            else if(state == FireFuelItemState.Embers)
            {

                if (extinguish)
                {
                    ReduceToAsh();
                    return;
                }

                currentPowerOutput = emberPowerOutput * CalculateEmberCurve() * fireSim.burnRateMult;
                currentPowerOutput *= Mathf.Pow(fireSim.airflow, AIRFLOW_EXP_EMBERS);
                //Main.Logger.Log($"Fuel {name} embers power output is {currentPowerOutput}", ComplexLogger.FlaggedLoggingLevel.Debug);

                if (remainingEnergy <= energyContent * 0.01f)
                {
                    ReduceToAsh();
                }
            }
            
            
            remainingEnergy -= currentPowerOutput * gameDeltaTime; //this may need adjustment, not sure yet, deltaTime always confuses me

            burnAmount = 1f - (remainingEnergy / energyContent);

        }

        public float CalculateCombustionCurve()
        {

            switch (type)
            {
                case FuelType.Wood:
                    return CalculateFirewoodCombustion();
                case FuelType.Stick:
                    return CalculateStickCombustion();
                case FuelType.Coal:
                     float t = Mathf.Clamp01(burnAmount / coalsThreshold);
                    return Mathf.Lerp(0.3f, 1f, t);
                case FuelType.Paper:
                    return Mathf.Pow(1f - burnAmount, 1.5f);
                case FuelType.Book:
                    return CalculateBookCombustion();
                case FuelType.Synthetic:
                    return CalculateFireLogCombustion();
                default: return CalculateFirewoodCombustion();
            }
        }

        private float CalculateFirewoodCombustion()
        {
            
            float start = 0.2f; //starting power fraction
            float end = 0.55f;  //power the curve ends at
            float ramp = 0.08f; //burn amount the curve starts declining at

            float curveProgress = 0;

            if(burnAmount < ramp)
            {
                
                curveProgress = Mathf.InverseLerp(0, ramp, burnAmount);
                return Mathf.Lerp(start, 1f, curveProgress);

            }
            else
            {
                curveProgress = Mathf.InverseLerp(ramp, coalsThreshold, burnAmount);
                return Mathf.Lerp(1f, end, curveProgress);
            }



        }
        private float CalculateFireLogCombustion()
        {

            float start = 0.5f; //starting power fraction
            float plateauDropPower = 0.75f;
            float rampEnd = 0.2f; 
            float ramp = coalsThreshold * rampEnd; //burn amount the plateau starts
            float plateauDrop = coalsThreshold * plateauDropPower; //burn amount the plateau starts dropping
            float end = 0.4f; //power the curve ends at

            float curveProgress = 0f;

            if (burnAmount < ramp) //ramping up
            {
                curveProgress = Mathf.InverseLerp(0, ramp, burnAmount);
                return Mathf.Lerp(start, 1f, curveProgress);
            }
            else if (burnAmount < plateauDrop) //plateau
            {
                return 1f;
            }
            else //decline
            {
                curveProgress = Mathf.InverseLerp(plateauDrop, coalsThreshold, burnAmount);
                return Mathf.Lerp(1f, end, curveProgress);
            }

        }

        private float CalculateStickCombustion()
        {

            float start = 0.5f; //starting power fraction
            float plateauDropPower = 0.8f;
            float rampEnd = 0.1f; 
            float ramp = coalsThreshold * rampEnd; //burn amount the plateau starts
            float plateauDrop = coalsThreshold * plateauDropPower; //burn amount the plateau starts dropping
            float end = 0.4f; //power the curve ends at

            float curveProgress = 0f;

            if (burnAmount < ramp)
            {
                 curveProgress = Mathf.InverseLerp(0, ramp, burnAmount);
                return Mathf.Lerp(start, 1f, curveProgress);
            }
            else if(burnAmount < plateauDrop)
            {
                return 1f;
            }
            else
            {
                curveProgress = Mathf.InverseLerp(plateauDrop, coalsThreshold, burnAmount);
                return Mathf.Lerp(1f, end, curveProgress);
            }

        }

        private float CalculateBookCombustion()
        {
            
            float start = 0.7f;
            float flareEnd = 0.05f;
            float settleEnd = 0.2f;
            float settleEndPower = 0.7f;
            float slowBurnEndPower = 0.6f;

            float curveProgress = 0f;

            if(burnAmount < flareEnd){ //flare up
            
                curveProgress = Mathf.InverseLerp(0, flareEnd, burnAmount);
                return Mathf.Lerp(start, 1f, curveProgress);
            }
            else if(burnAmount < settleEnd)
            {
                curveProgress = Mathf.InverseLerp(flareEnd, settleEnd, burnAmount);
                return Mathf.Lerp(1f, settleEndPower, curveProgress);
            }
            else
            {
                curveProgress = Mathf.InverseLerp(settleEnd, coalsThreshold, burnAmount);
                return Mathf.Lerp(settleEndPower, slowBurnEndPower, curveProgress);
            }
        }

        private float CalculateSlowDecayCurve()
        {
            coalsAmount = Mathf.InverseLerp(coalsThreshold, emberThreshold, burnAmount);

            float floor = coalsPowerOutput > 0f ? emberPowerOutput / coalsPowerOutput : 0.2f;
            return Mathf.Lerp(1f, floor, coalsAmount * coalsAmount);
        }

        private float CalculateEmberCurve()
        {

            embersAmount = Mathf.InverseLerp(emberThreshold, 1f, burnAmount);

                        return Mathf.Pow(
                   1f - embersAmount,
                   2f
               );
        }
        public float GetCharcoalProduced()
        {

            return gearItemRef.GetItemWeightKG().m_Units * charcoalYield * GetCarbonizationFactor();

        }

        private float GetCarbonizationFactor()
        {
            switch (state)
            {
                case FireFuelItemState.Burning:
                    return 0.05f;

                case FireFuelItemState.Coals:
                    return 0.50f;

                case FireFuelItemState.Embers:
                    return 0.80f;

                case FireFuelItemState.Ash:
                    return 0f;
            }

            return 0f;
        }

        public float GetRadiantPower()
        {
            switch (state)
            {
                case FireFuelItemState.Burning: return burningRadiantFraction * currentPowerOutput;
                case FireFuelItemState.Coals: return coalsRadiantFraction * currentPowerOutput;
                case FireFuelItemState.Embers: return embersRadiantFraction * currentPowerOutput;
                default: return 0f;
            }
        }

        public void ReduceToAsh()
        {
            //Main.Logger.Log("Reducing energy of fuel to 0.", ComplexLogger.FlaggedLoggingLevel.Debug);
            remainingEnergy = 0f;
            currentPowerOutput = 0f;
            state = FireFuelItemState.Ash;
        }

        //sim

        public FireFuelItem Clone() => (FireFuelItem)this.MemberwiseClone();


    }
}
