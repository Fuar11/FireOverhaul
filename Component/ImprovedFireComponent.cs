using Il2Cpp;
using ImprovedFires.Enums;
using ImprovedFires.Simulation;
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

        public FireSimulation fireSim;

        public bool changeFX = true;

        private const float MAX_STEPS = 200f;
        private const float  STEP_SIZE = 60F;
        private float durationEstimate = 0f;
        public void Start() { }

        public void Awake()
        {

            FireType type;

            Transform t = transform.parent != null ? transform.parent : transform;
            string name = t.gameObject.name.ToLowerInvariant();

            if(name.Contains("stove")) type = FireType.WoodStove;
            else if(name.ToLowerInvariant().Contains("barrel")) type = FireType.FireBarrel;
            else if(name.ToLowerInvariant().Contains("campfire")) type = FireType.Campfire;
            else if(name.ToLowerInvariant().Contains("fireplace")) type = FireType.Fireplace;
            else if(name.ToLowerInvariant().Contains("grill")) type = FireType.RimGrill;
            else if(name.ToLowerInvariant().Contains("forge")) type = FireType.Forge;
            else type = FireType.Campfire;

            fireSim = new FireSimulation(type);
        }

        public void Update() 
        {

            float gameDeltaTime = GameManager.GetTimeOfDayComponent().GetTODSeconds(Time.deltaTime);

            if (GameManager.m_IsPaused || GameManager.s_IsGameplaySuspended)
            {
                return;
            }

            fireSim.ambientTemperature = GameManager.GetWeatherComponent().GetCurrentTemperatureWithoutHeatSources();

            FireState oldState = fireSim.state;

            int i = 0;
            float timeRemaining = gameDeltaTime;
            float stepSize = Mathf.Max(STEP_SIZE, gameDeltaTime / MAX_STEPS);

            while(timeRemaining > 0 && i < MAX_STEPS)
            {
                float dt = Mathf.Min(timeRemaining, stepSize); 
                fireSim.Step(dt);
                timeRemaining -= dt;
                i++;
            }

            durationEstimate = Mathf.Max(0f, durationEstimate - gameDeltaTime);

            if(fireSim.state != oldState){ 
                changeFX = true;
                RefreshDurationEstimate();
            }


        }

        public void UpdateAirflow(float value)
        {
            fireSim.UpdateAirflow(value);
            RefreshDurationEstimate();
        }

        public float GetDurationEstimate() => durationEstimate;
        public void RefreshDurationEstimate()
        {
            durationEstimate = EstimateUsefulBurnSeconds();
        }
        private float EstimateUsefulBurnSeconds()
        {
                    
            FireSimulation clone = fireSim.Clone();

            float t = 0;
            while(clone.state > FireState.Embers && t < 43200) //if clone fire still isn't at embers in 12 in game hours, calculate how many seconds it's been running for
            {
                clone.Step(STEP_SIZE);
                t += STEP_SIZE;
            }

            return t;
        }

       public string GetAirflowLabel()
        {
            if(fireSim.type == FireType.WoodStove) return  "Damper";
            else return "Airflow";
        }

    }
}
