﻿using Il2Cpp;
using MelonLoader;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using HarmonyLib;
using System.Collections;
using ImprovedSignalVoid.GearSpawns;
using Il2CppTLD.Gameplay.Tunable;
using Il2CppNodeCanvas.Tasks.Conditions;
using Il2CppParadoxNotion.Services;
using Main;

namespace ImprovedSignalVoid.Patches.Patches
{
    internal class ShortwavePatches : MonoBehaviour
    {

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.Update))]

        internal class ShortwaveFPHActivator
        {
            private static void Postfix(PlayerManager __instance)
            {

                GameObject rig = GameObject.Find("CHARACTER_FPSPlayer/NEW_FPHand_Rig/GAME_DATA/Origin/HipJoint/Chest_Joint/Camera_Weapon_Offset/Shoulder_Joint/Shoulder_Joint_Offset/Right_Shoulder_Joint_Offset/RightClavJoint/RightShoulderJoint/RightElbowJoint/RightWristJoint/RightPalm/right_prop_point");
                GameObject shortwaveFPH = rig.transform.GetChild(16).gameObject;

                if (__instance.m_Gear.name.Contains("GEAR_SignalVoid"))
                {
                    if (__instance.m_InspectModeActive)
                    {
                        SetTriggerItem(__instance.m_Gear.name);
                        shortwaveFPH.SetActive(true);
                    }
                }

            }

            private static void SetTriggerItem(string gearItem)
            {

                if (gearItem.Contains("Tale1ChiefNote1")) return;

                if (gearItem.Contains("GEAR_SignalVoid"))
                {
                    GameObject sideTale1 = GameObject.Find("sideTale1");

                    if (sideTale1 != null)
                    {

                        MelonLogger.Msg("FOUND SIDETALE1");

                        MessageRouter msgRouter = sideTale1.GetComponent<MessageRouter>();

                        if (msgRouter == null)
                        {
                            MelonLogger.Msg("Message Router is null");
                            return;
                        }

                        if (msgRouter.listeners.ContainsKey("OnCustomEvent"))
                        {
                            var list = msgRouter.listeners["OnCustomEvent"];

                            Condition_PlayerHasInventoryItems condition = list[0].Cast<Condition_PlayerHasInventoryItems>();

                            condition.requirementsDict["_std"][0].name = gearItem;
                        }
                        else
                        {
                            MelonLogger.Msg("Unable to find key/value");
                        }

                    }
                    else
                    {
                        MelonLogger.Msg("CAN'T FIND SIDETALE1");

                    }
                }
                else
                {
                    MelonLogger.Msg("Gear is not narrative trigger item");
                }
            }

        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.ProcessPickupItemInteraction))]

        internal class ShortwaveFPHDeactivator
        {

            private static void Postfix(PlayerManager __instance)
            {

                GameObject rig = GameObject.Find("CHARACTER_FPSPlayer/NEW_FPHand_Rig/GAME_DATA/Origin/HipJoint/Chest_Joint/Camera_Weapon_Offset/Shoulder_Joint/Shoulder_Joint_Offset/Right_Shoulder_Joint_Offset/RightClavJoint/RightShoulderJoint/RightElbowJoint/RightWristJoint/RightPalm/right_prop_point");
                GameObject shortwaveFPH = rig.transform.GetChild(16).gameObject;

                if (__instance.m_Gear.name.Contains("GEAR_SignalVoid"))
                {
                    //wait 5 seconds
                    MelonCoroutines.Start(DisableShortwaveFPH(shortwaveFPH));
                }
            }

            //unsure
            private static IEnumerator DisableShortwaveFPH(GameObject shortwaveFPH)
            {

                if (shortwaveFPH.active)
                {
                    float waitSeconds = 5f;
                    for (float t = 0f; t < waitSeconds; t += Time.deltaTime) yield return null;
                    shortwaveFPH.SetActive(false);
                }
            }

        }


        [HarmonyPatch(typeof(GameManager), nameof(GameManager.Update))]

        internal class ShortwaveInSceneManager
        {

            private static void Postfix()
            {

                //Removes collider from object so only the collectible can be interacted with
                GameObject shortwaveActual = GameObject.Find("GEAR_HandheldShortwave");
                BoxCollider bc = shortwaveActual.GetComponent<BoxCollider>();
                if (bc) Destroy(bc);

                Inventory inv = GameManager.GetInventoryComponent();
                if (inv.GetBestGearItemWithName("GEAR_HandheldShortwave"))
                {
                    shortwaveActual.SetActive(false);
                }
            }
        }

        [HarmonyPatch(typeof(QualitySettingsManager), nameof(QualitySettingsManager.ApplyCurrentQualitySettings))]

        internal class ShortwaveInAirfieldSceneManager
        {
            private static void Postfix()
            {
                SaveDataManager sdm = Implementation.sdm;
                MelonLogger.Msg("GETTING REGION FROM MOD DATA TO SET AIRFIELD SPAWNS");
                string taleScene = sdm.LoadTaleStartRegion("startRegion");

                for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
                {
                    UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

                    if (scene.name == "AirfieldRegion_SANDBOX")
                    {

                        GameObject[] rootObjects = scene.GetRootGameObjects();
                        GameObject parent = null;

                        foreach (var obj in rootObjects)
                        {

                            if (obj.name == "Design")
                            {
                                parent = obj;
                                break;
                            }
                        }

                        if (parent != null)
                        {

                            GameObject tales = null;
                            GameObject trackables = null;

                            for (int j = 0; j < parent.transform.childCount; j++)
                            {
                                GameObject child = parent.transform.GetChild(j).gameObject;

                                if (child.name == "Tales")
                                {
                                    tales = child;
                                }
                                else if (child.name == "TrackableHiddenCaches")
                                {
                                    trackables = child;
                                }
                            }

                            if (tales != null)
                            {

                                if (ExperienceModeManager.GetCurrentExperienceModeType() == ExperienceModeType.Interloper || (ExperienceModeManager.GetCurrentExperienceModeType() == ExperienceModeType.Custom
                                   && GameManager.GetCustomMode().m_BaseWorldDifficulty == CustomTunableLMHV.Low))
                                {
                                    DisableObjectForXPMode component = tales.GetComponent<DisableObjectForXPMode>();
                                    Destroy(component);
                                }

                                if (!scene.name.Contains(taleScene))
                                {
                                    MelonLogger.Msg("Scene to start Tale is NOT Airfield");
                                    tales.transform.GetChild(0).gameObject.SetActive(false);
                                }
                               
                            }
                            else
                            {
                                MelonLogger.Msg("Unable to find tales object in scene");
                            }

                            if (trackables != null)
                            {

                                if (ExperienceModeManager.GetCurrentExperienceModeType() == ExperienceModeType.Interloper || (ExperienceModeManager.GetCurrentExperienceModeType() == ExperienceModeType.Custom
                                && GameManager.GetCustomMode().m_BaseWorldDifficulty == CustomTunableLMHV.Low))
                                {
                                    DisableObjectForXPMode component = trackables.GetComponent<DisableObjectForXPMode>();
                                    Destroy(component);
                                }
                            }
                            else
                            {
                                MelonLogger.Msg("Unable to find trackables object in scene");
                            }

                        }

                    }
                }
            }
        }
    }
}
