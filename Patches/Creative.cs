﻿using FG.Common.LevelEditor.Serialization;
using FG.Common;
using FGClient;
using HarmonyLib;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem;
using Il2CppSystem.Collections;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Text;
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
using Il2CppSystem.Threading;
using UnityEngine.Playables;
using FraggleExpansion;

namespace FraggleExpansion.Patches.Creative
{
    public class FeaturesPatches
    {
        /*[HarmonyPatch(typeof(FallguyCustomisationHandler), nameof(FallguyCustomisationHandler.UpdateCostumeOption)), HarmonyPrefix]
        public static bool UpdateCostumeOption(FallguyCustomisationHandler __instance, CostumeOption selectedCostumeOption, bool isTeamCostume)
        {
            if (selectedCostumeOption.DisplayName == "Builder")
            {
                // custom custumes?
            }
            return true;
        }*/

        // - CustomisationSelections 'customisations':  Customisations: ColourName=Colour_055 PatternName=Pattern_S08_27 CostumeTopName=BurgerBear_Top_01 CostumeBottomName=Fox_Bottom_01 CostumeFullName=NoneFullOption FaceplateName=faceplate_s10_03 VictoryPoseName=Victory_001 NicknameName=nickname_ss2_05 NameplateName=nameplate_ss02_event_trickerortreater EmoteNames=Emote_JumpRope/Emote_WaveA/Emote_Orcarina/Emote_Swiftlet
        [HarmonyPatch(typeof(LevelEditorManager), nameof(LevelEditorManager.InitialiseLocalCharacter)), HarmonyPrefix]
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
        }

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

        [HarmonyPatch(typeof(LevelEditorPlaceableObject), nameof(LevelEditorPlaceableObject.CanBeDeleted)), HarmonyPrefix]
        public static bool DeletionForBraindeadStartLine(LevelEditorPlaceableObject __instance, out bool __result)
        {
            switch (__instance.ObjectDataOwner.name)
            {
                case "POD_Rule_Floor_Start_Revised_Vanilla": // classic
                case "POD_Rule_Floor_Start_Retro": // digital
                case "POD_Rule_FloorStart_Vanilla": // beta
                case "POD_Rule_Floor_Start_Survival": // survival
                    if (Main.Instance.CountStartLines() <= 1) // last one
                    {
                        __result = false;
                        return false;
                    }
                    break;
                case "POD_Rule_Floor_End_Revised_Vanilla": // classic
                case "POD_Rule_Floor_End_Retro": // digital
                case "POD_Rule_FloorEnd_Vanilla": // beta
                    Main.Instance.CountEndLines();
                    break;
            }
            __result = __instance.IsActionValid(LevelEditorPlaceableObject.Action.Delete);
            return false;
        }

        [HarmonyPatch(typeof(LevelLoader), nameof(LevelLoader.PostLoadObjects)), HarmonyPostfix]
        public static void PostLoadObjects(LevelLoader __instance)
        {
            Main.Instance.CountStartLines(); // just so you won't have to hover a startline after loading a map
            Main.Instance.CountEndLines();
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

        [HarmonyPatch(typeof(LevelEditorManager), nameof(LevelEditorManager.SetupMapBoundsAndVisuals)), HarmonyPostfix] // fires after set_MapPlacementBounds
        public static void MapPlacementBounds(LevelEditorManager __instance, Vector3 mapSize)
        {
            if (FraggleExpansionData.BypassBounds)
            {
                __instance.MapPlacementBounds = new Bounds(__instance.MapPlacementBounds.center, new Vector3(100000, 100000, 100000));
            }
        }

        // ook, this removes the "startline has changed" text but nothing more, it still restricts to 20 players...
        /*[HarmonyPatch(typeof(LevelEditorManagerProxy), nameof(LevelEditorManagerProxy.CheckStartlineUpdateNotifications)), HarmonyPrefix]
        [HarmonyPatch(new[] { typeof(int), typeof(int) })] // overload match
        public static bool CheckStartlineUpdateNotifications(LevelEditorManagerProxy __instance, out int prevStartPoints, out int currentStartPoints)
        {
            prevStartPoints = 60;
            currentStartPoints = 60;
            return false;
        }*/

        // just unlocks the slider
        [HarmonyPatch(typeof(LevelEditorOptionsSliderSet), nameof(LevelEditorOptionsSliderSet.SoftLockSliderValue), MethodType.Setter), HarmonyPrefix]
        public static bool Unlock_maxplayers(LevelEditorOptionsSliderSet __instance, int value)
        {
            if (value == 20 || value == 0) { return false; }
            return true;
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
                Main.Instance.SetUp();
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

        // LevelEditorManagerIO.SelectedMusic
        /*[HarmonyPatch(typeof(UGCLevelDataSchema), nameof(UGCLevelDataSchema.LevelMusic), MethodType.Getter), HarmonyPrefix] // field accessor ok
        public static bool LevelMusic(UGCLevelDataSchema __instance, string __result)
        {
            Main.Instance.Log.LogMessage(__result);
            //__instance.LevelMusic = "MUS_InGame_Bean_Thieves";
            __result = "MUS_InGame_Bean_Thieves";
            return true;
        }*/

        [HarmonyPatch(typeof(LevelEditor.LevelEditorMultiSelectionHandler), nameof(LevelEditor.LevelEditorMultiSelectionHandler.AddToSelection)), HarmonyPostfix]
        public static void AddToSelection(LevelEditorMultiSelectionHandler __instance, LevelEditorPlaceableObject obj, int options, bool record = true, bool unselect = false)
        {
            bool SelectAllofType = Input.GetKey(KeyCode.LeftControl);
            bool SelectAll = Input.GetKey(KeyCode.LeftShift);
            if ((SelectAll || SelectAllofType) && record)
            {
                var stuff = UnityEngine.Object.FindObjectsOfType<LevelEditorPlaceableObject>();
                foreach (LevelEditorPlaceableObject o in stuff)
                {
                    if (o.ParentObject != null)
                    {
                        continue; // it's a trap
                    }
                    else if (SelectAllofType && SelectAll)
                    {
                        if (obj.name == o.name && (obj.transform.localScale == o.transform.localScale))
                        {
                            __instance.AddToSelection(o, options, false);
                        }
                    }
                    else if (SelectAllofType)
                    {
                        if (obj.name == o.name)
                        {
                            __instance.AddToSelection(o, options, false);
                        }
                    }
                    else
                    {
                        __instance.AddToSelection(o, options, false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LevelEditor.LevelEditorMultiSelectionHandler), nameof(LevelEditor.LevelEditorMultiSelectionHandler.RemoveFromSelection)), HarmonyPostfix]
        public static void RemoveFromSelection(LevelEditorMultiSelectionHandler __instance, LevelEditorPlaceableObject obj, int options, bool record = true, bool unselect = false)
        {
            if (Input.GetKey(KeyCode.LeftControl) && record) // hold it
            {
                var stuff = UnityEngine.Object.FindObjectsOfType<LevelEditorPlaceableObject>();
                foreach (LevelEditorPlaceableObject o in stuff)
                {
                    if (obj.name == o.name)
                    {
                        if (Input.GetKey(KeyCode.LeftShift) && (obj.transform.localScale != o.transform.localScale))
                        {
                            continue;
                        }
                        __instance.RemoveFromSelection(o, options, false, true);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LevelEditorPlaceableObject), nameof(LevelEditorPlaceableObject.AddScalingFeature)), HarmonyPrefix]
        public static bool AddScalingFeature(LevelEditorPlaceableObject __instance)
        {
            return !FraggleExpansionData.RemoveDefaultScalingFeature;
        }

    }

    public class BugFixes
    {
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

        //void Levels.Obstacles.COMMON_SelfRespawner::AddRespawnFallbackTransform(UnityEngine.Transform fallbackRespawnTransform)
        //- __instance: PB_BasketFall_LevelEditor(Clone) (Levels.Obstacles.COMMON_SelfRespawner)
        //- Parameter 0 'fallbackRespawnTransform': SpawnPoint2(UnityEngine.Transform)

        [HarmonyPatch(typeof(COMMON_SelfRespawner), nameof(COMMON_SelfRespawner.SetRespawnTransformAndOffset)), HarmonyPostfix]
        public static void SetRespawnTransformAndOffset(COMMON_SelfRespawner __instance, Transform respawnTransform, Vector3 respawnOffset)
        {
            //var transf = __instance._respawnTransform; //SpawnPoint1 2 3
            //Main.Instance.Log.LogMessage(respawnTransform.name);
            //Main.Instance.Log.LogMessage("old local: " + respawnTransform.localScale);
            //Main.Instance.Log.LogMessage("old lossy: " + respawnTransform.lossyScale);
            if (ShouldFixScaleByDetaching(respawnTransform.localScale, respawnTransform.lossyScale)) return;
            else if (!ShouldFixScale(respawnTransform.localScale, respawnTransform.lossyScale)) return;
            else if (__instance.transform.localScale != Vector3.one)
            { 
                Main.Instance.Log.LogMessage("Doc, my child " + respawnTransform.name + " is sick: " + __instance.transform.localScale);
                __instance.transform.localScale = Vector3.one; // we can fix him
            }
            respawnTransform.localScale = DoFixScale(respawnTransform.localScale, respawnTransform.lossyScale);
            //Main.Instance.Log.LogMessage("new local: " + respawnTransform.localScale);
        }

        [HarmonyPatch(typeof(Transform), nameof(Transform.localScale), MethodType.Setter), HarmonyPostfix]
        public static void Scale_set(Transform __instance, Vector3 value)
        {
            if (__instance.name == "Placeable_Obstacle_SpawnBasket_Vanilla_MEDIUM(Clone)")
            {
                System.Collections.Generic.List<GameObject> listOfChildren = Tools.FindAllChildren(__instance, "SpawnPoint1").Concat(Tools.FindAllChildren(__instance, "SpawnPoint2")).Concat(Tools.FindAllChildren(__instance, "SpawnPoint3")).ToList();
                foreach (GameObject n in listOfChildren)
                {
                    if (n.transform.childCount == 0) continue;
                    //Main.Instance.Log.LogMessage(n.transform.name);
                    //Main.Instance.Log.LogMessage("SetS old local: " + n.transform.localScale);
                    //Main.Instance.Log.LogMessage("SetS old lossy: " + n.transform.lossyScale);
                    if (ShouldFixScaleByDetaching(n.transform.localScale, n.transform.lossyScale))
                    {
                        //Main.Instance.Log.LogMessage("detaching...");
                        //Transform Parent = n.transform.parent;
                        //n.transform.SetParent(null, true);
                        //n.transform.localScale = Vector3.one;
                        //n.transform.SetParent(Parent, true); doesn't work?

                    }
                    else if (ShouldFixScale(n.transform.localScale, n.transform.lossyScale))
                    {
                        n.transform.localScale = DoFixScale(n.transform.localScale, n.transform.lossyScale);
                        //Main.Instance.Log.LogMessage("new local: " + n.transform.localScale);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LevelEditorDrawableData), nameof(LevelEditorDrawableData.SetBoxColliderSize)), HarmonyPrefix]
        public static bool SetBoxColliderSize(LevelEditorDrawableData __instance, ref Vector3 unseparatedSize, ref float snapSeparation)
        {
            if(FraggleExpansionData.GhostBlocks && unseparatedSize.y > 22.0F) // spooky moment detected
            {
                //Main.Instance.Log.LogMessage(__instance.name);
                //Main.Instance.Log.LogMessage("SetBoxColliderSize made SPOOKY " + unseparatedSize.y);
                unseparatedSize.y = 2.0F;
            }
            /*if (FraggleExpansionData.snapSeparatorSize != 0.025F) // I don't think this does anything useful
            {
                snapSeparation = FraggleExpansionData.snapSeparatorSize;
            }*/
            return true; // run the original f
        }

        [HarmonyPatch(typeof(LevelEditorActiveObjectBase), nameof(LevelEditorActiveObjectBase.CacheObjectPlacedPosAndRot)), HarmonyPostfix]
        public static void CacheObjectPlacedPosAndRot(LevelEditorActiveObjectBase __instance)
        {
            var Buoyancy = __instance.GetComponent<LevelEditorGenericBuoyancy>();
            if (Buoyancy)
                Buoyancy._placedPositionRotationCached = true; // no more floating away // MT you forgot this shiz!
        }
    }
 }
