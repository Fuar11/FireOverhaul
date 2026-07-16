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
    public class ImprovedFireComponent : MonoBehaviour
    {

        public FireState state;

        public List<FireFuelItem> fuelPool = new List<FireFuelItem>();

        public float totalFireDuration;
        public float totalFireTemperature;

        private float totalPower;

        private float BasePower = 4400f;       // watts
        private float BaseTemperature = 700f;  // degrees

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
            totalPower = 0;

            foreach (FireFuelItem fuel in fuelPool)
            {
                fuel.UpdateStats(this, extinguished);

                totalPower += fuel.currentPowerOutput;
            }

            float targetTemp = CalculateTemperature(totalPower);

            totalFireTemperature = Mathf.Lerp( //this may need some changing as well
                totalFireTemperature,
                targetTemp,
                Time.deltaTime * 0.1f);

            state = SetState();
            //do more effects based on temp
        }

        public void AddFuelItem(FireFuelItem item)
        {
            fuelPool.Add(item);
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

        private float CalculateTemperature(float power)
        {
            if (power <= 0)
                return 20f; // ambient, maybe modify this by the air temp

            float ratio = power / BasePower;

            float temp = BaseTemperature * Mathf.Pow(ratio, 0.35f);

            return Mathf.Clamp(temp, 20f, 1200f);
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
            
            if (burningCount == 0 && coalCount == 0 && emberCount > 0) return FireState.Embers;
            if (burningCount == 0 && coalCount == 0) return FireState.Coals;
            if (burningCount > 0 && totalCombustion < 0.5) return FireState.Dwindling;
            if (burningCount > 0 && totalCombustion < 1.5) return FireState.Steady;
            if (burningCount > 0 && totalCombustion > 1.5) return FireState.Raging;
            else return FireState.None;

        }

    }
}
