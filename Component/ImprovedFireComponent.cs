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

                totalPower += fuel.currentPowerOutput;
            }

            float targetTemp = CalculateTemperature();

            totalFireTemperature = Mathf.Lerp( //this may need some changing as well
                totalFireTemperature,
                targetTemp,
                gameDeltaTime * 0.1f);

            totalExternalFireTemperature = CalculateExternalTemperature();

            SetInnerAndOuterRadius();
         
            state = SetState();
            //do more effects based on temp
        }

        public void AddFuelItem(FireFuelItem item)
        {
            Main.Logger.Log($"Adding fuel item {item.name}", ComplexLogger.FlaggedLoggingLevel.Debug);
            fuelPool.Add(item);
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

        private FireState SetState()
        {

            totalCombustion =
                fuelPool
                .Where(x => x.state == Enums.FireFuelItemState.Burning)
                .Sum(x => x.CalculateCombustionCurve());

            burningCount = fuelPool.Count(f => f.state == Enums.FireFuelItemState.Burning);
            coalCount = fuelPool.Count(f => f.state == Enums.FireFuelItemState.Coals);
            emberCount = fuelPool.Count(f => f.state == Enums.FireFuelItemState.Embers);
            bool allAsh = fuelPool.All(f => f.state == Enums.FireFuelItemState.Ash); //energy remaining is almost 0

            if (burningCount == 0 && coalCount == 0 && emberCount > 0) return FireState.Embers;
            if (burningCount == 0 && coalCount == 0) return FireState.Coals;
            if (burningCount > 0 && totalCombustion < 0.5) return FireState.Dwindling;
            if (burningCount > 0 && totalCombustion < 1.5) return FireState.Steady;
            if (burningCount > 0 && totalCombustion > 1.5) return FireState.Raging;
            if (allAsh) return FireState.None;
            else return FireState.None;

        }

    }
}
