using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FireOverhaul.Component;
using FireOverhaul.Simulation;
using UnityEngine.UI;
using FireOverhaul.Utilities;

namespace FireOverhaul.Patches
{
    internal class PanelFirePatches
    {

        [HarmonyPatch(typeof(Panel_FireStart), nameof(Panel_FireStart.FilterItem))]
        public class RemoveBigFuelFromFireStart
        {
            public static void Postfix(ref bool __result, ref GearItem gi)
            {
                if (GameManager.GetSkillFireStarting().GetCurrentTierNumber() > 4) return;
                if (gi.name.ToLowerInvariant().Contains("hardwood") || gi.name.ToLowerInvariant().Contains("softwood") || gi.name.ToLowerInvariant().Contains("firelog") || gi.name.ToLowerInvariant().Contains("reclaimed") || gi.name.ToLowerInvariant().Contains("stick") || gi.name.ToLowerInvariant().Contains("candle")) __result = false;
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
                    FireOverhaulComponent? fire = GetFire(__instance);
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
                GameObject holder = new GameObject("FireOverhaul Slider Template");
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

            public static FireOverhaulComponent? GetFire(Panel_FeedFire panel)
            {
                if (panel.m_FireplaceInteraction == null || panel.m_FireplaceInteraction.Fire == null) return null;
                FireOverhaulComponent iFire = panel.m_FireplaceInteraction.Fire.GetComponent<FireOverhaulComponent>();
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
                FireOverhaulComponent? fire = Panel_FeedFire_AddSlider.GetFire(__instance);
                SliderRow? row = Panel_FeedFire_AddSlider.airflowRow;
                if (fire != null && row != null && row.Name != null) row.Name.text = fire.GetAirflowLabel();

                RemoveFuelButton.Update(__instance);
                CoreTempLabel.Update(__instance, fire);
			}

            private static void SetFireFillAmount(Panel_FeedFire __instance)
            {
                FireOverhaulComponent iFire = __instance.m_FireplaceInteraction.Fire.gameObject.GetComponent<FireOverhaulComponent>();

                if (iFire == null)
                {
                    Main.Logger.Log("FireOverhaulComponent is null, cannot get fire fill amount", ComplexLogger.FlaggedLoggingLevel.Error);
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

        // Shows the core fire temperature in the vanilla heat output stat, and renames its caption to match.
        internal static class CoreTempLabel
        {
            private const string CAPTION = "Core Temp";
            private const string VANILLA_CAPTION = "Heat Output";

            private static UILabel? caption;

            public static void Update(Panel_FeedFire panel, FireOverhaulComponent? fire)
            {
                if (fire == null || panel.m_Label_FireTemperature == null) return;

                panel.m_Label_FireTemperature.text = Utils.GetHeatIncreaseText(fire.fireSim.totalFireTemperature, false, panel.m_FireplaceInteraction.Fire);

                if (caption == null) RenameCaption(panel.m_Label_FireTemperature);
            }

            private static void RenameCaption(UILabel value)
            {
                // Nearest match first, in case the panel shows the same words somewhere else.
                for (Transform current = value.transform.parent; current != null; current = current.parent)
                {
                    foreach (UILabel label in current.GetComponentsInChildren<UILabel>(true))
                    {
                        if (label.text == null || !label.text.Trim().Equals(VANILLA_CAPTION, StringComparison.OrdinalIgnoreCase)) continue;

                        UILocalize localize = label.GetComponent<UILocalize>();
                        if (localize != null) UnityEngine.Object.DestroyImmediate(localize); //it would put the vanilla text back

                        label.text = CAPTION;
                        caption = label;
                        return;
                    }

                    if (current.GetComponent<Panel_FeedFire>() != null) return;
                }
            }
        }

        internal static class RemoveFuelButton
        {
            private const float OFFSET_FROM_TEMPLATE = -201f;

            private static GameObject? button;

            public static void Update(Panel_FeedFire panel)
            {
                if (button == null) Build(panel);
                if (button == null) return;

                FireOverhaulComponent? fire = Panel_FeedFire_AddSlider.GetFire(panel);
                bool show = fire != null && fire.fireSim.HasRemovableFuel();
                if (button.activeSelf != show) button.SetActive(show);
            }

            private static void Build(Panel_FeedFire panel)
            {
                Transform? template = FindTemplate(panel);

                if (template == null || FindClickable(template.gameObject) == null) return;

                GameObject holder = new GameObject("FireOverhaul Button Holder");
                holder.SetActive(false);

                GameObject copy = UnityEngine.Object.Instantiate(template.gameObject, holder.transform, false);
                copy.name = "Button_RemoveUnburntFuel";

                foreach (GenericButtonMouseSpawner spawner in copy.GetComponentsInChildren<GenericButtonMouseSpawner>(true))
                {
                    UnityEngine.Object.DestroyImmediate(spawner);
                }
                foreach (UILocalize localize in copy.GetComponentsInChildren<UILocalize>(true))
                {
                    UnityEngine.Object.DestroyImmediate(localize);
                }

                copy.transform.SetParent(template.parent, false);
                UnityEngine.Object.Destroy(holder);

                foreach (Transform node in copy.GetComponentsInChildren<Transform>(true))
                {
                    node.gameObject.layer = template.gameObject.layer;
                }

                Vector3 position = template.localPosition;
                copy.transform.localPosition = new Vector3(position.x + OFFSET_FROM_TEMPLATE, position.y, position.z);

                foreach (UILabel label in copy.GetComponentsInChildren<UILabel>(true))
                {
                    label.text = "Remove Unburnt Fuel";
                }

                UIButton clickable = FindClickable(copy)!;
                clickable.onClick.Clear();
                EventDelegate.Set(clickable.onClick, new Action(() => OnClick(panel)));
                clickable.isEnabled = true;

                copy.SetActive(false);
                button = copy;
            }

            private static Transform? FindTemplate(Panel_FeedFire panel)
            {
                foreach (Transform node in panel.GetComponentsInChildren<Transform>(true))
                {
                    if (node.name == "Button_Action_Secondary" && IsUnderMouse(node)) return node;
                }
                return null;
            }

            private static bool IsUnderMouse(Transform node)
            {
                for (Transform current = node.parent; current != null; current = current.parent)
                {
                    if (current.name == "Buffer") return true;
                }
                return false;
            }

            private static UIButton? FindClickable(GameObject root)
            {
                foreach (UIButton candidate in root.GetComponentsInChildren<UIButton>(true))
                {
                    if (candidate.onClick.Count > 0) return candidate;
                }
                return null;
            }

            private static void OnClick(Panel_FeedFire panel)
            {
                Panel_FeedFire_AddSlider.GetFire(panel)?.RemoveUnburntFuel();
            }
        }

        [HarmonyPatch(typeof(Panel_FeedFire), nameof(Panel_FeedFire.RefreshFireDurationLabel))]

        internal class FireDurationLabelTest
        {
            
            public static void Postfix(Panel_FeedFire __instance)
            {
                
                FireOverhaulComponent iFire = __instance.m_FireplaceInteraction.Fire.gameObject.GetComponent<FireOverhaulComponent>();

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

        [HarmonyPatch(typeof(Panel_FeedFire), nameof(Panel_FeedFire.OnTakeTorch))]

        internal class RemoveFuelOnTakeTorch
        {
            
            private static void Postfix(Panel_FeedFire __instance)
            {
                
                FireOverhaulComponent iFire = __instance.m_FireplaceInteraction.Fire.gameObject.GetComponent<FireOverhaulComponent>();

                if(iFire == null) return;

                iFire.fireSim.TakeTorch();
                iFire.RefreshDurationEstimate();

            }
        }

        [HarmonyPatch(typeof(Panel_FireStart), nameof(Panel_FireStart.OnCharcoalHarvest))]

        internal class TakeUnburntFuelOnCharcoalHarvest
        {
            

            private static void Postfix(Panel_FireStart __instance)
            {
                FireOverhaulComponent iFire = __instance.m_FireplaceInteraction.Fire.gameObject.GetComponent<FireOverhaulComponent>();

                if(iFire == null) return;

                iFire.RemoveUnburntFuel();

            }

        }

        [HarmonyPatch(typeof(Panel_ActionPicker), nameof(Panel_ActionPicker.DismantleFireCompleteCallback))]

        internal class GetFuelBackWhenBreakdownCampfire
        {
            
            private static void Postfix(Panel_ActionPicker __instance, ref bool success)
            {
                if (success)
                {
                    
                    FireOverhaulComponent iFire = __instance.m_FireplaceInteractedWith.gameObject.GetComponent<FireOverhaulComponent>();

                    if(iFire == null) return;
                    iFire.RemoveUnburntFuel();

                }
            }

        }
    }
}
