using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImprovedFires.Component;
using ImprovedFires.Simulation;
using UnityEngine.UI;
using ImprovedFires.Utilities;

namespace ImprovedFires.Patches
{
    internal class PanelFirePatches
    {

        [HarmonyPatch(typeof(Panel_FireStart), nameof(Panel_FireStart.FilterItem))]
        public class RemoveBigFuelFromFireStart
        {
            public static void Postfix(ref bool __result, ref GearItem gi)
            {
                if (GameManager.GetSkillFireStarting().GetCurrentTierNumber() > 4) return;
                if (gi.name.ToLowerInvariant().Contains("hardwood") || gi.name.ToLowerInvariant().Contains("softwood") || gi.name.ToLowerInvariant().Contains("firelog") || gi.name.ToLowerInvariant().Contains("reclaimed") || gi.name.ToLowerInvariant().Contains("stick")) __result = false;
            }
        }

        [HarmonyPatch(typeof(Panel_FireStart), nameof(Panel_FireStart.GetSelectedTinder))]

        public class DelegateTinderToFuel
        {
            public static void Postfix(ref FuelSourceItem __result, ref Panel_FireStart __instance)
            {
                __result = null;
            }
        }

        [HarmonyPatch(typeof(Panel_FireStart), nameof(Panel_FireStart.Enable))]

        internal class Panel_FireStart_HideTinderOption
        {
            
            private static void Postfix(Panel_FireStart __instance)
            {
                
                //Vector3 pos = __instance.m_SelectTinder.gameObject.transform.position;

                __instance.m_SelectTinder.gameObject.SetActive(false);

                string tinder = __instance.m_SelectTinder.m_LabelName.text;

                __instance.m_SelectFuel.m_LabelName.text = tinder;


            }

        }


        [HarmonyPatch(typeof(Panel_FeedFire), nameof(Panel_FeedFire.Initialize))]

        internal class Panel_FeedFire_AddSlider
        {
            public static SliderRow? airflowRow;
            private static void Postfix(Panel_FeedFire __instance)
            {
                //setup slider
                GameObject? go = PlaceSlider(__instance.gameObject, "Airflow", new Vector3(-855f, -200f, 0f));
                if (go != null)
                { 
                    airflowRow = new SliderRow(go, "Airflow", 0f, 1f, 21, v => {
                    ImprovedFireComponent? fire = GetFire(__instance);
                    float shown = fire != null ? fire.fireSim.GetAirflow(v) : v;
                    return $"{shown:0%}";
                }, () => GetFire(__instance)?.fireSim.airflowControl ?? 1f, v => GetFire(__instance)?.UpdateAirflow(v));
                    if (airflowRow.Name != null)
                    {   //this moves the label towards the slider
                        Vector3 p = airflowRow.Name.transform.localPosition;
                        airflowRow.Name.transform.localPosition = new Vector3(p.x + 300f, p.y, p.z); 
                    }
                }
            }

            private static GameObject? sliderTemplate;
            private static GameObject? GetSliderTemplate()
            {
                if (sliderTemplate != null) return sliderTemplate;

                Panel_OptionsMenu options = InterfaceManager.LoadPanel<Panel_OptionsMenu>();
                if (options == null || options.m_FieldOfViewSlider == null) return null;

                // Inactive holder, so the copy's Awake doesn't run before we strip it.
                GameObject holder = new GameObject("ImprovedFires Slider Template");
                UnityEngine.Object.DontDestroyOnLoad(holder);
                holder.SetActive(false);

                GameObject slider = UnityEngine.Object.Instantiate(options.m_FieldOfViewSlider.gameObject, holder.transform, false);
                slider.name = "Slider";

                // The spawner would rebuild it as the vanilla FOV slider; UILocalize would put the vanilla text back.
                foreach (GenericSliderSpawner spawner in slider.GetComponentsInChildren<GenericSliderSpawner>(true))
                {
                    UnityEngine.Object.DestroyImmediate(spawner);
                }
                StripLocalize(slider);

                sliderTemplate = slider;
                return sliderTemplate;
            }
            private static GameObject? PlaceSlider(GameObject parent, string name, Vector3 localPosition)
            {
                GameObject? template = GetSliderTemplate();
                if (template == null) return null;

                GameObject row = UnityEngine.Object.Instantiate(template, parent.transform, false);
                row.name = name;
                foreach (Transform node in row.GetComponentsInChildren<Transform>(true))
                {
                    node.gameObject.layer = parent.layer;
                }
                row.transform.localPosition = localPosition;
                row.SetActive(true);
                return row;
            }

            private static void StripLocalize(GameObject root)
            {
                foreach (UILocalize localize in root.GetComponentsInChildren<UILocalize>(true))
                {
                    UnityEngine.Object.Destroy(localize);
                }
            }

            public static ImprovedFireComponent? GetFire(Panel_FeedFire panel)
            {
                if (panel.m_FireplaceInteraction == null || panel.m_FireplaceInteraction.Fire == null) return null;
                ImprovedFireComponent iFire = panel.m_FireplaceInteraction.Fire.GetComponent<ImprovedFireComponent>();
                return iFire == null ? null : iFire;
            }
        }


        [HarmonyPatch(typeof(Panel_FeedFire), nameof(Panel_FeedFire.Update))]
		internal class Panel_FeedFire_Update
		{

            static float fill;

			private static void Postfix(Panel_FeedFire __instance)
			{
                SetFireFillAmount(__instance);
				(__instance.m_Sprite_FireFill as UIBasicSprite).fillAmount = fill;

                Panel_FeedFire_AddSlider.airflowRow?.Refresh();
                ImprovedFireComponent? fire = Panel_FeedFire_AddSlider.GetFire(__instance);
                SliderRow? row = Panel_FeedFire_AddSlider.airflowRow;
                if (fire != null && row != null && row.Name != null) row.Name.text = fire.GetAirflowLabel();
			}

            private static void SetFireFillAmount(Panel_FeedFire __instance)
            {
                ImprovedFireComponent iFire = __instance.m_FireplaceInteraction.Fire.gameObject.GetComponent<ImprovedFireComponent>();

                if (iFire == null)
                {
                    Main.Logger.Log("ImprovedFireComponent is null, cannot get fire fill amount", ComplexLogger.FlaggedLoggingLevel.Error);
                    return;
                }

                float target;

                switch (iFire.fireSim.state)
                {
                    case Enums.FireState.Embers:    target = Band(0.02f, 0.10f, iFire.fireSim.emberPower,   0f,           300f); break;
                    case Enums.FireState.Coals:     target = Band(0.10f, 0.25f, iFire.fireSim.coalsPower, FireSimulation.COALS_ENTER_EXIT,    2000f); break;
                    case Enums.FireState.Dwindling: target = Band(0.25f, 0.50f, iFire.fireSim.flamePower, FireSimulation.DWINDLE_ENTER, FireSimulation.STEADY_ENTER); break;
                    case Enums.FireState.Steady:    target = Band(0.50f, 0.85f, iFire.fireSim.flamePower, FireSimulation.STEADY_ENTER,  FireSimulation.RAGING_ENTER); break;
                    case Enums.FireState.Raging:    target = Band(0.85f, 1.00f, iFire.fireSim.flamePower, FireSimulation.RAGING_ENTER,  18000f); break;
                    default:   target = 0f; break;
                }
    
                float k = 1f - Mathf.Exp(-Time.unscaledDeltaTime / 0.3f); // ~0.3 real seconds
                fill = Mathf.Lerp(fill, target, k);
            }

            private static float Band(float min, float max, float power, float powerLow, float powerHigh)
            {
                return Mathf.Lerp(min, max, Mathf.InverseLerp(powerLow, powerHigh, power));
            }

		}

        [HarmonyPatch(typeof(Panel_FeedFire), nameof(Panel_FeedFire.RefreshFireDurationLabel))]

        internal class FireDurationLabelTest
        {
            
            public static void Postfix(Panel_FeedFire __instance)
            {
                
                ImprovedFireComponent iFire = __instance.m_FireplaceInteraction.Fire.gameObject.GetComponent<ImprovedFireComponent>();

                float num = iFire.GetDurationEstimate() / 60f;

                if(iFire.fireSim.state == Enums.FireState.Embers)
                {
                    __instance.m_Label_EstimatedFireDuration.text = Localization.Get("GAMEPLAY_Embers");
                    return;
                }

                int durationMinutes = Mathf.FloorToInt(num);

                if (num < 1f)
                {
                    durationMinutes = Mathf.CeilToInt(num);
                }

                string durationString = Il2Cpp.Utils.GetDurationString(durationMinutes);
		        __instance.m_Label_EstimatedFireDuration.text = durationString;
            }

        }
    }
}
