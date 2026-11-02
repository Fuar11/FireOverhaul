using Il2Cpp;
using ImprovedFires.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using FireState = ImprovedFires.Enums.FireState;

namespace ImprovedFires.Component
{
    [RegisterTypeInIl2Cpp]
    public class ImprovedFireComponent : MonoBehaviour
    {

        public FireState state;

        public List<FireFuelItem> fuelPool = new List<FireFuelItem>();

        private float d_estimatedFireDurationSeconds;

        public float totalExternalFireTemperature;
        public float totalFireTemperature;

        private float totalPower;
        public float burnRateMult;

        public float innerRadius;
        public float outerRadius;

        public const float BASELINE_POWER = 4500f; // watts
        public const float BASELINE_INTERNAL_TEMP = 700f;  // degrees
        public const float BASELINE_EXTERNAL_TEMP = 30f;  // degrees

        private bool extinguished = false;

        public bool changeFX = true;

        //state fields
        int burningCount;
        int coalCount;
        int emberCount;

        float totalCombustion;

        public void Start() { }

        public void Awake() { }

        public void Update() 
        {

            float gameDeltaTime = GameManager.GetTimeOfDayComponent().GetTODSeconds(Time.deltaTime);

            if (GameManager.m_IsPaused || GameManager.s_IsGameplaySuspended)
            {
                return;
            }

            totalPower = 0;
            burnRateMult = Mathf.Clamp(totalFireTemperature / BASELINE_INTERNAL_TEMP, 0.25f, 3f); //these values may need adjusting

            foreach (FireFuelItem fuel in fuelPool)
            {
                fuel.UpdateStats(this, extinguished);

                if(fuel.state == Enums.FireFuelItemState.Ash)
                {
                    RemoveFuelItem(fuel);
                    continue;
                }

                totalPower += fuel.currentPowerOutput;
            }

            float targetTemp = CalculateTemperature();

            totalFireTemperature = Mathf.Lerp( //this may need some changing as well
                totalFireTemperature,
                targetTemp,
                gameDeltaTime * 0.1f);

            totalExternalFireTemperature = CalculateExternalTemperature();

            SetInnerAndOuterRadius();
         
            if(state != FireState.Starting && state != FireState.None)
            {
                state = SetState();
                //do more effects based on temp
            }
           

            d_estimatedFireDurationSeconds = GetEstimatedFireDuration();

        }

        public void AddFuelItem(FireFuelItem item, int multi)
        {
            Main.Logger.Log($"Adding fuel item {item.name}", ComplexLogger.FlaggedLoggingLevel.Debug);
            for (int i = 0; i < multi; i++)
            {
                fuelPool.Add(item);
            }
        }

        public void RemoveFuelItem(FireFuelItem item)
        {
            Main.Logger.Log($"Removing fuel item {item.name}", ComplexLogger.FlaggedLoggingLevel.Debug);
            fuelPool.Remove(item);
        }

        public int GetCharcoal()
        {

            float charcoalAmount = 0f;

            foreach (FireFuelItem fuel in fuelPool)
            {
                fuel.UpdateStats(this, extinguished);

                charcoalAmount += fuel.GetCharcoalProduced();
            }

            return Mathf.CeilToInt(charcoalAmount / 0.1f);

        }

        private float CalculateTemperature()
        {
            if (totalPower <= 0)
                return 20f; // ambient, maybe modify this by the air temp

            float ratio = totalPower / BASELINE_POWER;

            float temp = BASELINE_INTERNAL_TEMP * Mathf.Pow(ratio, 0.35f);

            return Mathf.Clamp(temp, 20f, 1200f);
        }

        public float CalculateExternalTemperature()
        {
                   return
            (totalPower / BASELINE_POWER)
            * BASELINE_EXTERNAL_TEMP;
        }

        private void SetInnerAndOuterRadius()
        {

            float heatScale = totalExternalFireTemperature / 30f;
            innerRadius = Mathf.Clamp(1.5f * heatScale, 0.5f, 3f);
            outerRadius = Mathf.Clamp(4f * heatScale, 2f, 8f);
        }


        public float GetEstimatedFireDuration() //this needs MAJOR changes
        {
            float longestDuration = 0f;

            foreach (var fuel in fuelPool)
            {
                float duration = fuel.CalculateEstimatedRemainingDurationInSeconds(this);

                if (duration > longestDuration)
                    longestDuration = duration;
            }

            return longestDuration;
        }

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

        private FireState SetState() //really shitty state manager
        {

            FireState fireState = FireState.None;

            totalCombustion =
                fuelPool
                .Where(x => x.state == Enums.FireFuelItemState.Burning)
                .Sum(x => x.CalculateCombustionCurve());

            burningCount = fuelPool.Count(f => f.state == Enums.FireFuelItemState.Burning);
            coalCount = fuelPool.Count(f => f.state == Enums.FireFuelItemState.Coals);
            emberCount = fuelPool.Count(f => f.state == Enums.FireFuelItemState.Embers);
            //bool allAsh = fuelPool.Count(f => f.state == Enums.FireFuelItemState.Ash) == fuelPool.Count; //energy remaining is almost 0

            if (burningCount == 0 && coalCount == 0 && emberCount > 0){ 
                
                //Main.Logger.Log("All fuel is embers. Setting fire state to embers!", ComplexLogger.FlaggedLoggingLevel.Debug);
                fireState = FireState.Embers;
            }
            else if (burningCount == 0 && coalCount > 0) {
                //Main.Logger.Log("All fuel is coals. Setting fire state to coals!", ComplexLogger.FlaggedLoggingLevel.Debug);
                fireState = FireState.Coals;
            }
            else if (burningCount > 0 && totalCombustion < 0.5) {
                //Main.Logger.Log("Fire is dwindling. Setting fire state to dwindling!", ComplexLogger.FlaggedLoggingLevel.Debug);
                fireState = FireState.Dwindling;
            }
            else if (burningCount > 0 && totalCombustion < 1.5) {
                //Main.Logger.Log("Fire is steady. Setting fire state to steady!", ComplexLogger.FlaggedLoggingLevel.Debug);
                fireState = FireState.Steady;
            }
            else if (burningCount > 0 && totalCombustion > 1.5) {
                //Main.Logger.Log("Fire is raging. Setting fire state to raging!", ComplexLogger.FlaggedLoggingLevel.Debug);
                fireState = FireState.Raging;
            }
            else if (burningCount == 0 && coalCount == 0 && emberCount == 0) //fire has no energy releasing fuel left
            {
                //Main.Logger.Log("All fuel is ash. Setting fire state to none!", ComplexLogger.FlaggedLoggingLevel.Debug);
                fireState = FireState.None;
            }
            else {
                //Main.Logger.Log("No appropriate state found.", ComplexLogger.FlaggedLoggingLevel.Debug);
            }

            if( fireState != FireState.None) changeFX = true; 
            return fireState;
        }

    }
}
