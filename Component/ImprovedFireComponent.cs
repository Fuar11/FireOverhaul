using Il2Cpp;
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
            fireSim = new FireSimulation();
        }

        public void Update() 
        {

            float gameDeltaTime = GameManager.GetTimeOfDayComponent().GetTODSeconds(Time.deltaTime);

            if (GameManager.m_IsPaused || GameManager.s_IsGameplaySuspended)
            {
                return;
            }

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

        public void UpdateAirflow(float value) //call this
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

       

    }
}
