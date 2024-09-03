using FG.Common.LevelEditor.Serialization;
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
using System.Xml.XPath;
using System.Reflection.Emit;
using System.ComponentModel;
using BepInEx.Unity.IL2CPP.Utils;
using Il2CppSystem.Runtime.Serialization.Formatters.Binary;
using static Il2CppSystem.Linq.Expressions.Interpreter.CastInstruction.CastInstructionNoT;

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

        //int LevelEditorPlaceableObject::GetCost(bool includeCostOfGroupObjects, bool forClone)

        // CMSFraggleBudgetSchema
        // CMSFraggleObstacleSettings
        //[HarmonyPatch(typeof(PlaceableObjectCostHandler), nameof(PlaceableObjectCostHandler.GetBaseCost))]

        //[HarmonyPatch(typeof(CMSFraggleBudgetSchema), nameof(CMSFraggleBudgetSchema.Stock), MethodType.Setter)] // disable icon
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(LevelEditorPlaceableObject), nameof(LevelEditorPlaceableObject.GetCost))]
        public static System.Collections.Generic.IEnumerable<CodeInstruction> Return_Minus_One(System.Collections.Generic.IEnumerable<CodeInstruction> instructions)
        {
            var codes = new System.Collections.Generic.List<CodeInstruction>(2)
            {
                new CodeInstruction(OpCodes.Ldc_I4_M1), // push -1
                new CodeInstruction(OpCodes.Ret)
            };
            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(LevelEditorPlaceableObject), nameof(LevelEditorPlaceableObject.IsFloorOverlappingFloor), MethodType.Getter)]
        public static System.Collections.Generic.IEnumerable<CodeInstruction> Return_False(System.Collections.Generic.IEnumerable<CodeInstruction> instructions)
        {
            var codes = new System.Collections.Generic.List<CodeInstruction>(2)
            {
                new CodeInstruction(OpCodes.Ldc_I4_0), // push false (0)
                new CodeInstruction(OpCodes.Ret)
            };
            return codes.AsEnumerable();
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(PlaceableObjectCostHandler), nameof(PlaceableObjectCostHandler.GetRemainingStockCount))] // fix multiselect copy
        public static System.Collections.Generic.IEnumerable<CodeInstruction> Return_9999(System.Collections.Generic.IEnumerable<CodeInstruction> instructions)
        {
            var codes = new System.Collections.Generic.List<CodeInstruction>(2)
            {
                new CodeInstruction(OpCodes.Ldc_I4, 9999), // push int
                new CodeInstruction(OpCodes.Ret)
            };
            return codes.AsEnumerable();
        }

        /*[HarmonyPatch(typeof(LevelEditorPlaceableObject), nameof(LevelEditorPlaceableObject.CanBeDeleted)), HarmonyPrefix]
        public static bool CanBeDeleted(LevelEditorPlaceableObject __instance, out bool __result)
        {
            switch (__instance.ObjectDataOwner.name)
            {*/
                /*case "POD_Rule_Floor_Start_Revised_Vanilla": // classic
                case "POD_Rule_Floor_Start_Retro": // digital
                case "POD_Rule_FloorStart_Vanilla": // beta
                case "POD_Rule_Floor_Start_Survival": // survival
                case "POD_FloorStart_Survival_SpawnPoint_Vanilla": // survival point
                    if (Main.Instance.CountStartLines() <= 1) // last one
                    {
                        __result = false;
                        return false;
                    }
                    break;*/
                /*case "POD_Rule_Floor_End_Revised_Vanilla": // classic
                case "POD_Rule_Floor_End_Retro": // digital
                case "POD_Rule_FloorEnd_Vanilla": // beta
                    Main.Instance.CountEndLines();
                    break;
            }
            __result = __instance.IsActionValid(LevelEditorPlaceableObject.Action.Delete);
            return false;
        }*/

        /*[HarmonyPatch(typeof(LevelLoader), nameof(LevelLoader.ValidateUGCSchema)), HarmonyPostfix]
        public static void ValidateUGCSchema() // after post load objects
        {
            Main.Instance.SetUp(); // if we do it later than this - standard prefabs gonna get used for existing obj
        }*/

        [HarmonyPatch(typeof(LevelEditorManager), nameof(LevelEditorManager.GetStartAndEndPlatforms)), HarmonyPostfix]
        public static void GetStartAndEndPlatforms() // after ReloadSkybox()
        {
            if (!Main.Instance.Setup_done)
            {
                if (FraggleExpansionData.AddUnusedObjects)
                {
                    Main.Instance.AddCustomObjectsToCurrentList();
                }
                if (FraggleExpansionData.AddAllObjects)
                {
                    Main.Instance.AddAllObjectsToCurrentList();
                }
                //Main.Instance.SetUp(); // for newly-created levels we can do it this late
                if (FraggleExpansionData.BypassBounds)
                {
                    LevelEditorManager.Instance.MapPlacementBounds = new Bounds(LevelEditorManager.Instance.MapPlacementBounds.center, new Vector3(100000, 100000, 100000));
                }
                FraggleExpansionData.bWalls = LevelEditorWallControllerSettings.Instance.BetaWalls;
                FraggleExpansionData.bWallPillars = LevelEditorWallControllerSettings.Instance._combinedBetaPillars.Cast<PlaceableVariant_Wall>();
                Main.Instance.Setup_done = true;
                //Main.Instance.SetupStartLines();
                //Main.Instance.CountStartLines(); // just so you won't have to hover a startline after loading a map
                //Main.Instance.CountEndLines();
                var Btns = LevelEditorManager.Instance.UI._radialDefinition.RadialDefinitions;
                Btns[5]._nameLocKey = "Lesser vertical & rotational step";
                Btns[4]._nameLocKey = "Center Camera";
                Btns[2]._nameLocKey = "Ghost Blocks";
                Btns[2]._descriptionLocKey = "Increase floor height via `R` key above 20 to make it ghost";
                Btns[3]._descriptionLocKey = "Shift + Select = select all\nCtrl + Select = select all objects of the same type\nChrl + Shift + Select = select all objects of the same scale\n(console-key) ` + Select = select all in proximity\n` or 1 or 2 or 3 or 4 + DEselect = reset \\ + \\ - the proximity";
                Btns[1].SetToggleValue(myXml.Instance.Data.XPathSelectElement("/States/GridSnap").Value == "True"); // GridSnap
                Btns[4].SetToggleValue(myXml.Instance.Data.XPathSelectElement("/States/CameraCenter").Value == "True"); // CenterSelect
                Btns[2].SetToggleValue(myXml.Instance.Data.XPathSelectElement("/States/GhostBLocks").Value == "True"); // Clipping
                Btns[5].SetToggleValue(myXml.Instance.Data.XPathSelectElement("/States/Precision").Value == "True"); // Precision
            }
        }

        [HarmonyPatch(typeof(LevelEditor_RadialMenuButtonDefinition), nameof(LevelEditor_RadialMenuButtonDefinition.SetToggleValue)), HarmonyPostfix]
        public static void SetToggleValue(LevelEditor_RadialMenuButtonDefinition __instance, bool isOn)
        {
            //Main.Instance.Log.LogMessage(__instance.NameKey);
            switch(__instance.NameKey)
            {
                case "Ghost Blocks":
                    FraggleExpansionData.GhostBlocks = isOn;
                    myXml.Instance.Data.XPathSelectElement("/States/GhostBLocks").Value = isOn.ToString();
                    myXml.Instance.Save();
                    break;
                case "Center Camera":
                    myXml.Instance.Data.XPathSelectElement("/States/CameraCenter").Value = isOn.ToString();
                    myXml.Instance.Save();
                    break;
                case "wle_object_snap":
                    myXml.Instance.Data.XPathSelectElement("/States/GridSnap").Value = isOn.ToString();
                    myXml.Instance.Save();
                    break;
                case "Lesser vertical & rotational step":
                    myXml.Instance.Data.XPathSelectElement("/States/Precision").Value = isOn.ToString();
                    myXml.Instance.Save();
                    break;
                default:
                    Main.Instance.Log.LogMessage("LevelEditor_RadialMenuButtonDefinition -> SetToggleValue()  unknown button toggled: " + __instance.NameKey);
                    return;
            }
        }

        // ScriptableObjects.PlaceableVariant_Prefab LevelEditor.LevelEditorWallControllerSettings::GetWallPrefabFromLength(float desiredLength, bool useBetaWalls)
        //- __instance: WallControllerSettings(LevelEditor.LevelEditorWallControllerSettings)

        [HarmonyPatch(typeof(LevelEditorPlaceableObject), nameof(LevelEditorPlaceableObject.SelectObject)), HarmonyPostfix]
        public static void SelectObject(LevelEditorPlaceableObject __instance)
        {
            switch (__instance.name)
            {
                case "Placeable_Wall_Inflatable_Vanilla_Post_Combined_beta(Clone)":
                    LevelEditorWallControllerSettings.Instance._walls = FraggleExpansionData.bWalls;
                    LevelEditorWallControllerSettings.Instance._combinedPillars = FraggleExpansionData.bWallPillars;
                    break;
                case "Placeable_Wall_Inflate_Post_End(Clone)":
                    LevelEditorWallControllerSettings.Instance._walls = FraggleExpansionData.bWalls;
                    if (FraggleExpansionData.vWallPillars) LevelEditorWallControllerSettings.Instance._combinedPillars = FraggleExpansionData.vWallPillars;
                    break;
                case "Placeable_Wall_Inflatable_Retro_Post_Combined(Clone)":
                    if(!FraggleExpansionData.dWallPillars)
                    {
                        FraggleExpansionData.dWallPillars = __instance.ObjectDataOwner.objectVariants[0].Cast<PlaceableVariant_Wall>();
                        FraggleExpansionData.dWalls = new Il2CppSystem.Collections.Generic.List<PlaceableVariant_Wall>(5);
                        FraggleExpansionData.dWalls.Add(__instance.ObjectDataOwner.objectVariants[1].Cast<PlaceableVariant_Wall>());
                        FraggleExpansionData.dWalls.Add(__instance.ObjectDataOwner.objectVariants[2].Cast<PlaceableVariant_Wall>());
                        FraggleExpansionData.dWalls.Add(__instance.ObjectDataOwner.objectVariants[3].Cast<PlaceableVariant_Wall>());
                        FraggleExpansionData.dWalls.Add(__instance.ObjectDataOwner.objectVariants[4].Cast<PlaceableVariant_Wall>());
                        FraggleExpansionData.dWalls.Add(__instance.ObjectDataOwner.objectVariants[5].Cast<PlaceableVariant_Wall>());
                    }
                    LevelEditorWallControllerSettings.Instance._walls = FraggleExpansionData.dWalls;
                    LevelEditorWallControllerSettings.Instance._combinedPillars = FraggleExpansionData.dWallPillars;
                    break;
                case "Placeable_Wall_Inflatable_Vanilla_Post_Combined(Clone)":
                    if (!FraggleExpansionData.vWallPillars)
                    {
                        FraggleExpansionData.vWallPillars = __instance.ObjectDataOwner.objectVariants[0].Cast<PlaceableVariant_Wall>();
                        FraggleExpansionData.vWalls = new Il2CppSystem.Collections.Generic.List<PlaceableVariant_Wall>(5);
                        FraggleExpansionData.vWalls.Add(__instance.ObjectDataOwner.objectVariants[1].Cast<PlaceableVariant_Wall>());
                        FraggleExpansionData.vWalls.Add(__instance.ObjectDataOwner.objectVariants[2].Cast<PlaceableVariant_Wall>());
                        FraggleExpansionData.vWalls.Add(__instance.ObjectDataOwner.objectVariants[3].Cast<PlaceableVariant_Wall>());
                        FraggleExpansionData.vWalls.Add(__instance.ObjectDataOwner.objectVariants[4].Cast<PlaceableVariant_Wall>());
                        FraggleExpansionData.vWalls.Add(__instance.ObjectDataOwner.objectVariants[5].Cast<PlaceableVariant_Wall>());
                    }
                    LevelEditorWallControllerSettings.Instance._walls = FraggleExpansionData.vWalls;
                    LevelEditorWallControllerSettings.Instance._combinedPillars = FraggleExpansionData.vWallPillars;
                    break;
                default:
                    return;
            }
        }
    }

    public class BypassesPatches
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(LevelEditorMultiSelectionHandler), nameof(LevelEditorMultiSelectionHandler.CanSelectMore), MethodType.Getter)]
        [HarmonyPatch(typeof(LevelEditorStateReticleBase), nameof(LevelEditorStateReticleBase.CanPlaceSelectedObject))]
        [HarmonyPatch(typeof(PlaceableObjectCostHandler), nameof(PlaceableObjectCostHandler.HasStock))] // CanPlaceMoreOfThisObj
        public static System.Collections.Generic.IEnumerable<CodeInstruction> Return_False(System.Collections.Generic.IEnumerable<CodeInstruction> instructions)
        {
            var codes = new System.Collections.Generic.List<CodeInstruction>(2)
            {
                new CodeInstruction(OpCodes.Ldc_I4_1), // push true (1)
                new CodeInstruction(OpCodes.Ret)
            };
            return codes.AsEnumerable();
        }

        /*[HarmonyPatch(typeof(LevelEditor.LevelEditorMultiSelectionHandler), nameof(LevelEditor.LevelEditorMultiSelectionHandler.CanSelectMore), MethodType.Getter), HarmonyPrefix]
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
        }*/

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
            //Main.Instance.Log.LogMessage("SoftLockSliderValue: " + value + " from Max: " + __instance.Max); // Max is buggy okay...
            /*if (value == 0) // you broke it // MT fixed it??
            {
                __instance.ResetCurrentValue(); // we fixed it // not fully tho, you have to reselect everything now
                //__instance.ResetValue(); // doesn't help
                //__instance.Max = 40;
                //__instance.UpperValue = 40;
                //__instance.upperLimit = (Nullable<int>)40;
                return false;
            }*/
            if (value < 40) { return false; }
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
                if (!Main.Instance.Preprocessed) Main.Instance.Preproccess_POD_prefabs(); //  && (FraggleExpansionData.AddAllObjects || FraggleExpansionData.AddUnusedObjects)
            }
            else if (scene.name == "FallGuy_Editor")
            {
                //if (!Main.Instance.Preprocessed) Main.Instance.ManageAllCurrentObjects();
                Main.Instance.Setup_done = false;
                //Main.Instance.Preprocessed = false; // do we need to do it every map reload? - we don't
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

        // publish:
        // FG.Common.UGCNetworking.UGCNetworkRequests::CreateUpdateLevelStateRequest(FG.Common.UGCNetworking.UpdateLevelStateParameters parameters)
        // void LevelEditorLevel::Publish(bool tryUploadThumbnail, FG.Common.UGCNetworking.ResponseHandler customHandler)
        // save:
        // FG.Common.UGCNetworking.UGCNetworkRequests::CreateUpdateDraftRequest(FG.Common.UGCNetworking.UpdateDraftParameters parameters)
        // void LevelEditorLevel::UpdateDraft(FG.Common.UGCNetworking.ResponseHandler onCompleteCallBack)
        // public unsafe UpdateDraftParameters(string shareCode, string levelJSON, string levelName, string levelDescription, string levelTags, Dictionary<string, Il2CppSystem.Object> levelConfig)

        // the SODIUM :-)
        /*[HarmonyPatch(typeof(UGCJsonSerializer), nameof(UGCJsonSerializer.SerializeObject)), HarmonyPostfix] // or ConvertAndPerformLevelDataUpgradeIfRequired
        public static void music_sel_set(ref string __result, Il2CppSystem.Object value, bool indented = false)
        {
            //Main.Instance.Log.LogMessage(__result);
            if (FraggleExpansionData.LevelMusic.Length != 0)
            {
                __result = Regex.Replace(__result, "Music\":\"[^\\\"]*", "Music\":\"" + FraggleExpansionData.LevelMusic);
            }
            //Main.Instance.Log.LogMessage(__result);
        }*/

        // LevelEditorManagerIO.SelectedMusic
        /*[HarmonyPatch(typeof(UGCLevelDataSchema), nameof(UGCLevelDataSchema.LevelMusic), MethodType.Getter), HarmonyPrefix] // field accessor ok
        public static bool LevelMusic(UGCLevelDataSchema __instance, string __result)
        {
            Main.Instance.Log.LogMessage(__result);
            //__instance.LevelMusic = "MUS_InGame_Bean_Thieves";
            __result = "MUS_InGame_Bean_Thieves";
            return true;
        }*/

        public static float fDistance_threshold = 10.0f;
        [HarmonyPatch(typeof(LevelEditor.LevelEditorMultiSelectionHandler), nameof(LevelEditor.LevelEditorMultiSelectionHandler.AddToSelection)), HarmonyPostfix]
        public static void AddToSelection(LevelEditorMultiSelectionHandler __instance, LevelEditorPlaceableObject obj, int options, bool record = true, bool unselect = false)
        {
            bool SelectNearby = Input.GetKey(KeyCode.BackQuote);
            bool SelectAllofType = Input.GetKey(KeyCode.LeftControl);
            bool SelectAll = Input.GetKey(KeyCode.LeftShift);
            if ((SelectAll || SelectAllofType || SelectNearby) && record)
            {
                var stuff = UnityEngine.Object.FindObjectsOfType<LevelEditorPlaceableObject>();
                foreach (LevelEditorPlaceableObject o in stuff)
                {
                    if (o.transform.parent != null)
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
                    else if (SelectAll)
                    {
                        __instance.AddToSelection(o, options, false);
                    }
                    else if (SelectNearby && Vector3.Distance(obj.transform.position, o.transform.position) <= fDistance_threshold)
                    {
                        __instance.AddToSelection(o, options, false);
                        //Main.Instance.Log.LogMessage("added nearby object which is this far: " + Vector3.Distance(obj.transform.position, o.transform.position));
                    }
                }
            }
        }

        [HarmonyPatch(typeof(LevelEditor.LevelEditorMultiSelectionHandler), nameof(LevelEditor.LevelEditorMultiSelectionHandler.RemoveFromSelection)), HarmonyPrefix]
        public static bool RemoveFromSelection(LevelEditorMultiSelectionHandler __instance, LevelEditorPlaceableObject obj, int options, bool record = true, bool unselect = false)
        {
            if (record)
            {
                if (Input.GetKey(KeyCode.LeftControl)) // hold it
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
                else if (Input.GetKey(KeyCode.BackQuote))
                {
                    //Main.Instance.Log.LogMessage("RESET dist radius ");
                    fDistance_threshold = 10.0f;
                    //__instance.RemoveFromSelection(obj, options, false, unselect); // lets run it twice to update text
                }
                else if (Input.GetKey(KeyCode.Alpha1))
                {
                    //Main.Instance.Log.LogMessage("+10 dist radius ");
                    fDistance_threshold += 5.0f;
                    //__instance.RemoveFromSelection(obj, options, false, unselect); // lets run it twice to update text
                }
                else if (Input.GetKey(KeyCode.Alpha2))
                {
                    //Main.Instance.Log.LogMessage("-10 dist radius ");
                    fDistance_threshold -= 5.0f;
                    //__instance.RemoveFromSelection(obj, options, false, unselect); // lets run it twice to update text
                }
                else if (Input.GetKey(KeyCode.Alpha3))
                {
                    //Main.Instance.Log.LogMessage("+1 dist radius ");
                    fDistance_threshold += 1.0f;
                    //__instance.RemoveFromSelection(obj, options, false, unselect); // lets run it twice to update text
                }
                else if (Input.GetKey(KeyCode.Alpha4))
                {
                    //Main.Instance.Log.LogMessage("-1 dist radius ");
                    fDistance_threshold -= 1.0f;
                    //__instance.RemoveFromSelection(obj, options, false, unselect); // lets run it twice to update text
                }
            }
            return true;
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

        [HarmonyPatch(typeof(COMMON_SelfRespawner), nameof(COMMON_SelfRespawner.SetReferences), new[] { typeof(Transform), typeof(Vector3) }), HarmonyPostfix] // old SetRespawnTransformAndOffset
        public static void SetReferences(COMMON_SelfRespawner __instance, Transform respawnTransform, Vector3 respawnOffset)
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
            if(unseparatedSize.y > 22.0F) // spooky moment detected
            {
                //Main.Instance.Log.LogMessage(__instance.name);
                //Main.Instance.Log.LogMessage("SetBoxColliderSize made SPOOKY " + unseparatedSize.y);
                if (FraggleExpansionData.GhostBlocks || !Main.Instance.Setup_done) // only for newly placed objects, not ones loaded with the map
                { 
                    unseparatedSize.y = 2.0F; 
                }
                else // stop him, he's breaking the laws of physics!
                {
                    __instance.ApplyDepthToFloor(21, true);
                    return false;
                }
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
        // edit - it works for entering and leaving explore state but the map auto-saving when publishing still uses live position, not the cached one!!

        [HarmonyPatch(typeof(LevelEditorManager), nameof(LevelEditorManager.ShowPublishPopUp)), HarmonyPrefix]
        public static bool ShowPublishPopUp(LevelEditorManager __instance)
        {
            var flies = GameObject.FindObjectsOfType<LevelEditorGenericBuoyancy>();
            foreach (var fly in flies)
            {
                //Main.Instance.Log.LogMessage("changing pos from: " + fly._placedPosition.ToString() + "to: " + fly.CachedGameObject.transform.position.ToString());
                fly.ResetMoveAndRotate();
                fly.ResetBuoyancy();
            }
            return true;
        }

        /*[HarmonyPatch(typeof(LevelLoader), nameof(LevelLoader.InstantiateFromJSON)), HarmonyPostfix]
        public static void InstantiateFromJSON(LevelLoader __instance, UGCObjectDataSchema schema, bool includeIDs, GameObject __result) // after InstantiateObject -> this -> LoadObject_Create
        {
            var original = schema.Name;
            var resulting = __result.name;
            if(original != resulting) Main.Instance.Log.LogMessage("MISMATCH ERROR" + ": org: " + original + " , result: " + resulting + ", org GUID: " + schema.GUID);
        }*/
        // ok, this doesn't catch object corruptions, tested
    }
}
