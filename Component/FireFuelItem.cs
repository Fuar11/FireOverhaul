using ImprovedFires.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImprovedFires.Component
{
    public class FireFuelItem
    {
        public GearItem gearItemRef = null;
        public string name = "default fuel item";

        public FireFuelItemState state = FireFuelItemState.None;
        public FuelType type = FuelType.None;

        //definition
        public float energyContent;        // Total energy stored in joules
        public float maxPowerOutput;       // Maximum watts when burning at optimal rate
        public float coalsPowerOutput;     // Maximum watts when burning residual carbon
        public float emberPowerOutput;     // Remaining watts when emitting lasting ambient heat
        public float timeToIgniteAtTemp;   //0 = instant ignite
        public float ignitionTemp;         // Temperature at which combustion can commence
        public float coalsThreshold;       // Amount of total fuel consumption required for fuel to be burning as coals
        public float emberThreshold;       // Amount of total fuel consumption required for fuel to be burning as embers
        public float baseBurnDuration;
        public float charcoalYield;

        //percentages of consumption
        public float burnAmount = 0f;
        public float coalsAmount = 0f;
        public float embersAmount = 0f;

        public float remainingEnergy;

        public float currentPowerOutput;
        public float ignitionProgress = 0f;



        public FireFuelItem(string name, FuelType type, float baseDuration, float timeToIgniteAtTemp, float ignitionTemp, float maxPowerOutput, float energyContent, float coalsThreshold, float embersThreshold, float charcoalYield)
        {
            this.name = name;
            this.type = type;
            this.baseBurnDuration = baseDuration;
            this.timeToIgniteAtTemp = timeToIgniteAtTemp;
            this.ignitionTemp = ignitionTemp;
            this.maxPowerOutput = maxPowerOutput;
            this.energyContent = energyContent;
            this.coalsThreshold = coalsThreshold;
            this.embersAmount = embersThreshold;
            this.charcoalYield = charcoalYield;
        }

    
        public void UpdateStats(ImprovedFireComponent fire, bool extinguish)
        {

            if (state == FireFuelItemState.Cold)
            {
                ignitionProgress = 0f;
                if (fire.totalFireTemperature >= ignitionTemp)
                {
                    state = FireFuelItemState.Heating;
                }
            }

            if (state == FireFuelItemState.Heating)
            {

                if (fire.totalFireTemperature < ignitionTemp || extinguish) state = FireFuelItemState.Cold;

                float heatingRatio = Mathf.Clamp(fire.totalFireTemperature / ignitionTemp, 1f, 3f);
                float ignitionDuration = timeToIgniteAtTemp / heatingRatio;

                ignitionProgress += Time.deltaTime / ignitionDuration;
                if (ignitionProgress >= 1f)
                {
                    state = FireFuelItemState.Burning;
                }
            }

            if (state == FireFuelItemState.Burning)
            {
                currentPowerOutput = maxPowerOutput * CalculateCombustionCurve();

                if (burnAmount <= coalsThreshold) state = FireFuelItemState.Coals;
            }
            else if(state == FireFuelItemState.Coals)
            {
                currentPowerOutput = coalsPowerOutput * CalculateSlowDecayCurve();
                if (burnAmount <= coalsThreshold) state = FireFuelItemState.Embers;
            }
            else if(state == FireFuelItemState.Embers)
            {
                currentPowerOutput = emberPowerOutput * CalculateEmberCurve();

                if(remainingEnergy <= energyContent * 0.01f)
                {
                    state = FireFuelItemState.Ash;
                }
            }
            
            remainingEnergy -= currentPowerOutput * Time.deltaTime; //this may need adjustment, not sure yet, this always confuses me

            burnAmount = 1f - (remainingEnergy / energyContent);

        }

        public float CalculateCombustionCurve()
        {

            switch (type)
            {
                case FuelType.Wood:
                    return 4f * burnAmount * (1f - burnAmount);
                case FuelType.Stick:
                    return Mathf.Pow(1f - burnAmount, 0.5f);
                case FuelType.Coal:
                    return Mathf.Clamp01(
                       Mathf.Sin(burnAmount * Mathf.PI * 0.8f)
                   );
                case FuelType.Paper:
                    return Mathf.Pow(1f - burnAmount, 1.5f);
                case FuelType.Synthetic:
                    return CalculateFireLogCombustion();
                default: return 4f * burnAmount * (1f - burnAmount);
            }
        }

        private float CalculateFireLogCombustion()
        {
            if (burnAmount < 0.2f)
            {
                return Mathf.Lerp(0.5f, 1f, burnAmount / 0.2f);
            }

            if (burnAmount < 0.8f)
            {
                return 1f;
            }

            return Mathf.Lerp(1f, 0.2f, (burnAmount - 0.8f) / 0.2f);
        }


        private float CalculateSlowDecayCurve()
        {

            coalsAmount = Mathf.InverseLerp(coalsThreshold, emberThreshold, burnAmount);

            return Mathf.Lerp(
       1f,
       0f,
       Mathf.Pow(coalsAmount, 0.7f)
   );

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

    }
}
