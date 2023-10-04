﻿using FG.Common.LevelEditor.Serialization;
using FG.Common;
using FGClient;
using HarmonyLib;
using UnhollowerBaseLib;
using UnityEngine;
using Wushu.Framework.ExtensionMethods;
using FMODUnity;
using UnityEngine.SceneManagement;
using FG.Common.CMS;
using ScriptableObjects;
using FG.Common.Loadables;
using TreeView;
using LevelEditor;
using Levels.Obstacles;
using static LevelEditor.LevelEditorWallResizer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static RootMotion.FinalIK.RagdollUtility;
using FG.Common.UGCNetworking;
using System.Text.RegularExpressions;

namespace FraggleExpansion.Patches.Creative
{
    public class FeaturesPatches
    {

        /*[HarmonyPatch(typeof(LevelEditorManagerAudio), nameof(LevelEditorManagerAudio.StartGameplayMusic)), HarmonyPrefix]
        public static bool CustomMusicOnPublish(LevelEditorManagerAudio __instance)
        {
            if (FraggleExpansionData.CustomTestMusic)
            {
                if (!RuntimeManager.HasBankLoaded(FraggleExpansionData.MusicBankPlayMode))
                {
                    RuntimeManager.LoadBank(FraggleExpansionData.MusicBankPlayMode);
                    RuntimeManager.LoadBank(FraggleExpansionData.MusicBankPlayMode + ".assets");
                }

                AudioLevelEditorStateListener._instance._randomMusicEvent = FraggleExpansionData.MusicEventPlayMode;
                AudioLevelEditorStateListener._instance.OnStartGameplayMusic(new StartGameplayMusic());
            }
            return !FraggleExpansionData.CustomTestMusic;
        }*/

        [HarmonyPatch(typeof(LevelEditorStateExplore), nameof(LevelEditorStateExplore.DisableState)), HarmonyPrefix]
        public static bool LastPositionDisplayOnReticle(ILevelEditorState nextState)
        {
            if (FraggleExpansionData.LastPostion)
            {
                if (MiscData.CurrentPositionDisplay != null) UnityEngine.Object.Destroy(MiscData.CurrentPositionDisplay);
                MiscData.CurrentPositionDisplay = GameObject.CreatePrimitive(PrimitiveType.Cube);
                MiscData.CurrentPositionDisplay.name = "PositionDisplay";
                MiscData.CurrentPositionDisplay.GetComponent<MeshRenderer>().material = ThemeManager._currentTheme.FilletMaterial;
                MiscData.CurrentPositionDisplay.GetComponent<MeshRenderer>().material.color = new Color(0, 5, 5, 1);
                MiscData.CurrentPositionDisplay.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                MiscData.CurrentPositionDisplay.GetComponent<BoxCollider>().isTrigger = true;
                MiscData.CurrentPositionDisplay.transform.position = UnityEngine.Object.FindObjectOfType<FallGuysCharacterController>().transform.position;
            }
            UnityEngine.Object.FindObjectOfType<LevelEditorNavigationScreenViewModel>().SetPlayVisible(true);

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(LevelEditorStateTest), nameof(LevelEditorStateTest.Initialise))]
        [HarmonyPatch(typeof(LevelEditorStateExplore), nameof(LevelEditorStateExplore.Initialise))]
        public static bool LastPositionDisplayOnPlayState()
        {
            if (FraggleExpansionData.LastPostion)
            {
                if (MiscData.CurrentPositionDisplay != null) UnityEngine.Object.Destroy(MiscData.CurrentPositionDisplay);
            }

            UnityEngine.Object.FindObjectOfType<LevelEditorNavigationScreenViewModel>().SetPlayVisible(false);

            return true;
        }

        /*[HarmonyPatch(typeof(LevelEditorManager), nameof(LevelEditorManager.InitialiseLocalCharacter)), HarmonyPrefix]
        public static bool MainSkinInFraggle(LevelEditorManager __instance, GameObject playerGameObject, out FallGuysCharacterController characterController, out ClientPlayerUpdateManager playerUpdateManager)
        {
            if (FraggleExpansionData.UseMainSkinInExploreState)
            {
                var CustomisationSelection = GlobalGameStateClient.Instance.PlayerProfile.CustomisationSelections;
                CustomisationManager.Instance.ApplyCustomisationsToFallGuy(playerGameObject, CustomisationSelection, -1);
            }
            characterController = null;
            playerUpdateManager = null;
            return true;
        }*/

        [HarmonyPatch(typeof(LevelEditorPlaceableObject), nameof(LevelEditorPlaceableObject.CanBeDeleted)), HarmonyPrefix]
        public static bool DeletionForBraindeadStartLine(LevelEditorPlaceableObject __instance, out bool __result)
        {
            bool StartLineValidation = LevelEditorManager.Instance.CostManager.GetCount(__instance.ObjectDataOwner) > 1 && __instance.IsActionValid(LevelEditorPlaceableObject.Action.Delete);
            bool UseStartLineValidation = ThemeManager.CurrentThemeData.ObjectList.GetStartGantry() == __instance.ObjectDataOwner;
            __result = UseStartLineValidation ? StartLineValidation : __instance.IsActionValid(LevelEditorPlaceableObject.Action.Delete);
            return false;
        }
    }

    public class BypassesPatches
    {
        [HarmonyPatch(typeof(LevelEditor.LevelEditorMultiSelectionHandler), nameof(LevelEditor.LevelEditorMultiSelectionHandler.CanSelectMore), MethodType.Getter), HarmonyPrefix]
        public static bool RemoveMaxMultiSelect(out bool __result)
        {
            __result = true;
            return false;
        }

        [HarmonyPatch(typeof(LevelEditorStateReticleBase), nameof(LevelEditorStateReticleBase.CanPlaceSelectedObject)), HarmonyPrefix]
        public static bool Clipping(LevelEditorStateReticleBase __instance, out bool __result)
        {
            __result = true;
            return !FraggleExpansionData.CanClipObjects;
        }

        [HarmonyPatch(typeof(LevelLoader), nameof(LevelLoader.LoadObjects)), HarmonyPostfix]
        public static void BoundsOnExistingRound(LevelLoader __instance, Il2CppReferenceArray<UGCObjectDataSchema> schemas)
        {
            if (FraggleExpansionData.BypassBounds)
                LevelEditorManager.Instance.MapPlacementBounds = new Bounds(LevelEditorManager.Instance.MapPlacementBounds.center, new Vector3(100000, 100000, 100000));
        }
    }
    // by arg type // new[] { typeof(string), typeof(LoadSceneParameters) }
    // OnLevelEditorEnteredExploreModeEvent
    // OnLevelLoadSuccessEvent
    // LevelEditorPlaceableObject.SetActiveColliders(bool)
    // LevelEditorPlaceableObject.ColliderBounds
    public class MainFeaturePatches
    {
        // Transition
        // MainMenu
        // FallGuy_FraggleBackground_Vanilla
        // FallGuy_Editor
        [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.Internal_SceneLoaded)), HarmonyPostfix]
        public static void Internal_SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // do stuff
            if (scene.name == "MainMenu")
            {
                if (GameObject.Find("SeasonPassButton")) { GameObject.Find("SeasonPassButton").SetActive(false); }
                //if (GameObject.Find("ShopButton")) { GameObject.Find("ShopButton").SetActive(false); }
                //if (GameObject.Find("Generic_UI_PlayButton2_Prefab")) { GameObject.Find("Generic_UI_PlayButton2_Prefab").SetActive(false); }
                if (GameObject.Find("BottomRight_Group")) { GameObject.Find("BottomRight_Group").SetActive(false);  }
            }
            else if (scene.name == "FallGuy_Editor")
            {
                /*if (FraggleExpansionData.RemoveCostAndStock) // no effect
                {
                    new BudgetResourcesBarChanged(1500);
                    AudioLevelEditorStateListener._instance.OnResourcesBarChanged(new BudgetResourcesBarChanged(2000));
                }*/

                if (FraggleExpansionData.BypassBounds)
                    LevelEditorManager.Instance.MapPlacementBounds = new Bounds(LevelEditorManager.Instance.MapPlacementBounds.center, new Vector3(100000, 100000, 100000));

                if (FraggleExpansionData.AddUnusedObjects)
                {
                    //Log.LogMessage("Objects to add: " + FraggleExpansionData.AddObjectData.Length);
                    foreach (string sData in FraggleExpansionData.AddObjectData)
                    {
                        //Log.LogMessage("Adding object: " + sData);
                        Main.Instance.AddObjectToCurrentList(sData, LevelEditorPlaceableObject.Category.Advanced, 0, 0);
                    }
                }

                Main.Instance.ManageCostRotationStockForAllObjects(FraggleExpansionData.RemoveCostAndStock, FraggleExpansionData.RemoveRotation);

                Main.Instance.ManagePlaceableExtras();
            }

        }

        [HarmonyPatch(typeof(LevelEditorDrawableData), nameof(LevelEditorDrawableData.ApplyScaleToObject)), HarmonyPrefix]
        public static bool FixCheckpointZoneWithPainterScaling(LevelEditorDrawableData __instance, bool subObj = false)
        {
            // Basically there's issues if the semantic type is CheckpointFloor, but if it's not then the size of the Checkpoint Collider is not changed, so we do it before scaling the object here
            if (__instance.FloorType == LevelEditorDrawableData.DrawableSemantic.FloorObject && __instance.gameObject.GetComponent<LevelEditorCheckpointFloorData>() && __instance._checkpointZone != null)
                __instance._checkpointZone.SetCheckpointZoneColliderScale(__instance.GetShaderScale(), LevelEditorDrawableData.DrawableSemantic.FloorObject);

            return true;
        }

        [HarmonyPatch(typeof(LevelEditorCheckpointFloorData), nameof(LevelEditorCheckpointFloorData.UpdateChevron)), HarmonyPrefix]
        public static bool FixChevronScaling(ref Vector3 scale)
        {
            scale = scale.Abs();
            return true;
        }
    }

    public class BugFixes
    {
        /*[HarmonyPatch(typeof(LevelEditorLevel), nameof(LevelEditorLevel.FetchLevelJSON)), HarmonyPostfix]
        public static void FetchLevelJSON(LevelEditorLevel __instance, ResponseHandler customHandler)
        {
            Main.Instance.Log.LogMessage("Praying...");
            //Main.Instance.Log.LogMessage(customHandler.ToString());
            //Main.Instance.Log.LogMessage(customHandler.data);
        }*/
        /*[HarmonyPatch(typeof(LevelEditorLevel), nameof(LevelEditorLevel._levelJSON), MethodType.Getter), HarmonyPostfix]
        public static void set_Name(LevelEditorLevel __instance, string value)
        {
            Main.Instance.Log.LogMessage("Praying...");
            //Main.Instance.Log.LogMessage(__instance._customName);
            //Main.Instance.Log.LogMessage(__instance._levelJSON);
            //Main.Instance.Log.LogMessage(customHandler.ToString());
            //Main.Instance.Log.LogMessage(customHandler.data);
        }*/

        public static Vector3 InitialVector = new Vector3(0.01F, 0.01F, 0.01F);
        public static bool ShouldFixScale(Vector3 local, Vector3 lossy)
        {
            if (local == lossy) return false;
            if (lossy == InitialVector) return false; // yea no, you can't make it all 0.01 now
            
            return true;
        }
        public static bool ShouldFixScaleByDetaching(Vector3 local, Vector3 lossy)
        {
            if (Mathf.Approximately(lossy.x, 0.0F) || Mathf.Approximately(lossy.y, 0.0F) || Mathf.Approximately(lossy.z, 0.0F)) return true;

            return false;
        }

        public static Vector3 DoFixScale(Vector3 local, Vector3 lossy)
        {
            return new Vector3(1.0F / (lossy.x / local.x), 1.0F / (lossy.y / local.y), 1.0F / (lossy.z / local.z));
        }

        [HarmonyPatch(typeof(COMMON_SelfRespawner), nameof(COMMON_SelfRespawner.Start)), HarmonyPostfix]
        public static void Scale_start(COMMON_SelfRespawner __instance)
        {
            var transf = __instance._respawnTransform; //SpawnPoint1 2 3
            if (ShouldFixScaleByDetaching(transf.localScale, transf.lossyScale)) return;
            if (!ShouldFixScale(transf.localScale, transf.lossyScale)) return;

            transf.localScale = DoFixScale(transf.localScale, transf.lossyScale);
        }

        [HarmonyPatch(typeof(Transform), nameof(Transform.localScale), MethodType.Setter), HarmonyPostfix]
        public static void Scale_set(Transform __instance, UnityEngine.Vector3 value)
        {
            if (__instance.name == "Placeable_Obstacle_SpawnBasket_Vanilla_MEDIUM(Clone)")
            {
                List<GameObject> listOfChildren = Tools.FindAllChildren(__instance, "SpawnPoint1").Concat(Tools.FindAllChildren(__instance, "SpawnPoint2")).Concat(Tools.FindAllChildren(__instance, "SpawnPoint3")).ToList();
                foreach (GameObject n in listOfChildren)
                {
                    if (ShouldFixScaleByDetaching(n.transform.localScale, n.transform.lossyScale))
                    {
                        /*Main.Instance.Log.LogMessage("detaching...");
                        Transform Parent = n.transform.parent;
                        n.transform.SetParent(null, true);
                        n.transform.localScale = Vector3.one;
                        //n.transform.SetParent(Parent, true); doesn't work?
                        */
                    }
                    else if (ShouldFixScale(n.transform.localScale, n.transform.lossyScale))
                    {
                        n.transform.localScale = DoFixScale(n.transform.localScale, n.transform.lossyScale);
                    }
                }
            }
        }

        // the SODIUM :-)
        [HarmonyPatch(typeof(UGCJsonSerializer), nameof(UGCJsonSerializer.SerializeObject)), HarmonyPostfix]
        public static void music_sel_set(ref string __result, Il2CppSystem.Object value, bool indented = false)
        {
            //Main.Instance.Log.LogMessage(__result);
            if (FraggleExpansionData.LevelMusic.Length != 0)
            {
                __result = Regex.Replace(__result, "Music\":\"[^\\\"]*", "Music\":\"" + FraggleExpansionData.LevelMusic);
            }
            //Main.Instance.Log.LogMessage(__result);
        }
        
        /*[HarmonyPatch(typeof(UGCLevelDataSchema), nameof(UGCLevelDataSchema.LevelMusic), MethodType.Getter), HarmonyPrefix] // field accessor ok
        public static bool LevelMusic(UGCLevelDataSchema __instance, string __result)
        {
            Main.Instance.Log.LogMessage(__result);
            //__instance.LevelMusic = "MUS_InGame_Bean_Thieves";
            __result = "MUS_InGame_Bean_Thieves";
            return true;
        }*/
    }
 }
