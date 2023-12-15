using BepInEx;
using BepInEx.IL2CPP;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using FGClient;
using FG.Common;
using FG.Common.Fraggle;
using FMODUnity;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using FG.Common.CMS;
using System.Collections.Generic;
using ScriptableObjects;
using System.Net;
using System;
using Levels.Obstacles;
using TreeView;
using FMOD.Studio;
using FG.Common.Loadables;
using UnityEngine.UI;
using FraggleExpansion.Patches.Creative;
using FraggleExpansion.Patches.Reticle;
using FraggleExpansion.Patches;
using static UnityEngine.UI.GridLayoutGroup;
using FGClient.UI.Core;
using FGClient.UI;
using static FG.Common.MetricConstants;
using Mediatonic.Tools.Utils;
using UnityEngine.Animations;
using Il2CppSystem;
using static PerformanceData;
using UnityEngine.Playables;
using MPG.Utility;
using static RootMotion.FinalIK.RagdollUtility;

namespace FraggleExpansion
{
    [BepInPlugin("FraggleExpansion", "Creative Expansion Pack CE", "2.4")]
    public class Main : BepInEx.Unity.IL2CPP.BasePlugin
    {
        public Harmony _Harmony = new Harmony("com.simp.fraggleexpansion");
        public static Main Instance;
        PlaceableObjectData BetaStart, DigitalStart, ClassicStart, SurvivalStart, BetaEnd, DigitalEnd, ClassicEnd;
        //public SlimeGamemodesManager _SlimeGamemodeManager;

        public override void Load()
        {
            Log.LogMessage("Creative Expansion Pack CE 2.4");

            Instance = this;

            new PropertiesReader();
            //_SlimeGamemodeManager = new SlimeGamemodesManager();

            // Requirement to Intialize Creative Expansion Pack
            //_Harmony.PatchAll(typeof(Requirements)); // lithum bs

            // Within Creative Patches
            _Harmony.PatchAll(typeof(MainFeaturePatches));
            _Harmony.PatchAll(typeof(FeaturesPatches));
            _Harmony.PatchAll(typeof(BypassesPatches));

            // UI Stuff
            _Harmony.PatchAll(typeof(ReticleUI));

            // Misc.
            _Harmony.PatchAll(typeof(BugFixes));
            //_Harmony.PatchAll(typeof(MiscPatches));
            //Log.LogMessage("RUN: Patches DONE");
        }

        int itemID = 1000;
        public void SetUp()
        {
            PropertiesReader.Instance.InitializeData(); // re-read config every map load
            //if (FraggleExpansionData.BypassBounds) // this here does nothing
            //    LevelEditorManager.Instance.MapPlacementBounds = new Bounds(LevelEditorManager.Instance.MapPlacementBounds.center, new Vector3(100000, 100000, 100000));
            
            if (FraggleExpansionData.AddUnusedObjects)
            {
                //Log.LogMessage("Objects to add: " + FraggleExpansionData.AddObjectData.Length);
                foreach (string sData in FraggleExpansionData.AddObjectData)
                {
                    //Log.LogMessage("Adding object: " + sData);
                    AddObjectToCurrentList(sData, LevelEditorPlaceableObject.Category.Advanced,0, itemID);
                    itemID++;
                }
            }

            if (FraggleExpansionData.AddAllObjects)
            {
                AddAllObjectsToCurrentList();
            }

            ManageCostRotationStockForAllObjects(FraggleExpansionData.RemoveCostAndStock, FraggleExpansionData.RemoveRotation);

            ManagePlaceableExtras();
        }

        public void ManagePlaceableExtras()
        {
            foreach (var Placeable in LevelEditorObjectList.CurrentObjects.Cast<Il2CppSystem.Collections.Generic.List<PlaceableObjectData>>())
            {
                var Prefab = Placeable.defaultVariant.Prefab;

                if (Prefab.GetComponent<LevelEditorDrawableData>())
                {
                    var Drawable = Prefab.GetComponent<LevelEditorDrawableData>();

                    if (Prefab.GetComponent<LevelEditorCheckpointFloorData>())
                    {
                        Drawable._painterMaxSize = new Vector3(30, 30, 30);
                        Drawable._canBePainterDrawn = true;
                        Drawable.FloorType = LevelEditorDrawableData.DrawableSemantic.FloorObject;
                        Drawable._restrictedDrawingAxis = LevelEditorDrawableData.DrawRestrictedAxis.Up;

                        UnityEngine.Object.Destroy(Prefab.GetComponent<LevelEditorFloorScaleParameter>());
                        Prefab.GetComponent<LevelEditorPlaceableObject>().hasParameterComponents = false;
                    }
                    if (FraggleExpansionData.InsanePainterSize)
                    {
                        Drawable._painterMaxSize = new Vector3(100000, 100000, 100000);
                    }
                    if (Input.GetKey(KeyCode.F4)) // hold it // not sure if you still need the hotkey
                    {
                        FraggleExpansionData.GhostBlocks = !FraggleExpansionData.GhostBlocks;
                        //Il2CppSystem.Nullable<EventInstance> AudioEvent = AudioManager.CreateAudioEvent("SFX_Emote_ExpressiveDance");
                        //AudioEvent.Value.start();
                    }
                    if (FraggleExpansionData.GhostBlocks)
                    {
                        Drawable.DrawableDepthMaxIncrements = 100000;
                    }
                    else
                    {
                        Drawable.DrawableDepthMaxIncrements = 20;
                    }
                }
                
                if (Placeable.name == "POD_Rule_FloorStart_Vanilla")
                {
                    BetaStart = Placeable;
                }
                else if (Placeable.name == "POD_Rule_Floor_Start_Retro")
                {
                    DigitalStart = Placeable;
                }
                else if (Placeable.name == "POD_Rule_Floor_Start_Revised_Vanilla")
                {
                    ClassicStart = Placeable;
                }
                else if (Placeable.name == "POD_Rule_Floor_Start_Survival")
                {
                    SurvivalStart = Placeable;
                    Placeable.defaultVariant.Prefab.GetComponent<LevelEditorPlaceableObject>().EditorOnlyRenderers = null; // this makes it visible
                    Placeable.defaultVariant.Prefab.GetComponent<LevelEditorPlaceableObject>().collidersToDisable = null; // this makes it touchable
                    //Placeable.defaultVariant.Prefab.GetComponent<LevelEditorPlaceableObject>().ParameterTypes = LevelEditorParametersManager.LegacyParameterTypes.None;
                }
                else if (Placeable.name == "POD_Rule_FloorEnd_Vanilla")
                {
                    BetaEnd = Placeable;
                }
                else if (Placeable.name == "POD_Rule_Floor_End_Retro")
                {
                    DigitalEnd = Placeable;
                }
                else if (Placeable.name == "POD_Rule_Floor_End_Revised_Vanilla")
                {
                    ClassicEnd = Placeable;
                }
                /*else if (Placeable.name == "POD_SeeSaw_Vanilla")
                {
                    Placeable.category = LevelEditorPlaceableObject.Category.MovingSurfaces;
                }
                else if (Placeable.name == "POD_Special_Goo_Slide")
                {
                    Placeable.category = LevelEditorPlaceableObject.Category.Platforms;
                }*/
                /*if (Prefab.GetComponent<LevelEditorDrawablePremadeWallSurface>())
                {
                    var DrawableWallSurface = Prefab.GetComponent<LevelEditorDrawablePremadeWallSurface>();
                    DrawableWallSurface._useBetaWalls = FraggleExpansionData.BetaWalls && ThemeManager.CurrentThemeData.ID != "THEME_RETRO";
                }*/

                /*if (Prefab.name == "POD_SpawnBasket_Vanilla")
                {
                    var ParameterComponent = Prefab.GetComponent<LevelEditorCarryTypeParameter>();
                    foreach (var CarryType in ParameterComponent._carryTypes)
                    {
                        //CarryType.CarryPrefab.GetComponent<COMMON_SelfRespawner>().KillPlaneYThreshold = 75;
                        CarryType.CarryPrefab.AddComponent<ScaleConstraint>();
                    }
                }*/

                /*if ((Placeable.name == "POD_Floating_Cannon_Revised_Vanilla" && ThemeManager.CurrentThemeData.ID != "THEME_RETRO") || (Placeable.name == "POD_Floating_Cannon_Retro" && ThemeManager.CurrentThemeData.ID == "THEME_RETRO"))
                {
                    Prefab.AddComponent<LevelEditorReceiver>();
                    Prefab.GetComponentInChildren<CannonActiveStateEventResponders>()._eventResponderNameKey = "wle_event_responder_toggle_on_off";
                }*/ // it makes maps not load

                /*var Buoyancy = Prefab.GetComponent<LevelEditorGenericBuoyancy>();
                if (Buoyancy)
                    UnityEngine.Object.Destroy(Buoyancy);*/ // no more floating up and down
                                                            //Buoyancy._placedPositionRotationCached = true; // no more floating away - doesn't work like this
            }
        }
        
        public int CountStartLines()
        {
            int nClassic, nDigital, nBeta, nSurvival;
            if (ClassicStart) { nClassic = LevelEditorManager.Instance.CostManager.GetCount(ClassicStart); } else { nClassic = 0; }
            if (DigitalStart) { nDigital = LevelEditorManager.Instance.CostManager.GetCount(DigitalStart); } else { nDigital = 0; }
            if (BetaStart) { nBeta = LevelEditorManager.Instance.CostManager.GetCount(BetaStart); } else { nBeta = 0; }
            if (SurvivalStart) { nSurvival = LevelEditorManager.Instance.CostManager.GetCount(SurvivalStart); } else { nSurvival = 0; }
            //Log.LogMessage("found starts: " + (nClassic + nDigital + nBeta + nSurvival));
            if (nClassic > 0) { ThemeManager.CurrentStartGantry = ClassicStart; }
            else if (nDigital > 0) { ThemeManager.CurrentStartGantry = DigitalStart; }
            else if (nBeta > 0) { ThemeManager.CurrentStartGantry = BetaStart; }
            else if (nSurvival > 0) { ThemeManager.CurrentStartGantry = SurvivalStart; }
            return nClassic + nDigital + nBeta + nSurvival;
        }

        public void CountEndLines()
        {
            if (ClassicEnd && LevelEditorManager.Instance.CostManager.GetCount(ClassicEnd) > 0) { ThemeManager.CurrentFinishGantry = ClassicEnd; }
            if (DigitalEnd && LevelEditorManager.Instance.CostManager.GetCount(DigitalEnd) > 0) { ThemeManager.CurrentFinishGantry = DigitalEnd; }
            if (BetaEnd && LevelEditorManager.Instance.CostManager.GetCount(BetaEnd) > 0) { ThemeManager.CurrentFinishGantry = BetaEnd; }
            //Log.LogMessage("found ends: " + nClassic + " " + nDigital + " " + nBeta);
        }

        public void ManageCostRotationStockForAllObjects(bool RemoveCostAndStock, bool RemoveRotation)
        {
            foreach (var Placeable in LevelEditorObjectList.CurrentObjects.Cast<Il2CppSystem.Collections.Generic.List<PlaceableObjectData>>())
            {
                RemoveCostAndRotationForObject(Placeable, RemoveCostAndStock, RemoveRotation);

                if (!RemoveCostAndStock) return;

                var DefaultVariantPrefab = Placeable.defaultVariant.Prefab;

                if (Placeable.name == "POD_Wheel_Maze_Revised_Common" || Placeable.name == "POD_WheelMaze")
                {
                    LevelEditorCostOverrideWheelMaze Comp = DefaultVariantPrefab.GetComponent<LevelEditorCostOverrideWheelMaze>();
                    Comp._chevronPatternModifier._costModifier = 0;
                    Comp._diamondPatternModifier._costModifier = 0;
                    Comp._hexagonPatternModifier._costModifier = 0;
                    Comp._hourglassPatternModifier._costModifier = 0;
                    Comp._rhomboidPatternModifier._costModifier = 0;
                    Comp._largeSizeModifier._costModifier = 0;
                    Comp._smallSizeModifier._costModifier = 0;
                    Comp._mediumSizeModifier._costModifier = 0;
                    Comp._trianglePatternModifier._costModifier = 0;
                }

                if (DefaultVariantPrefab.GetComponent<LevelEditorDrawablePremadeWallSurface>())
                {
                    var DrawableWallSurface = DefaultVariantPrefab.GetComponent<LevelEditorDrawablePremadeWallSurface>();
                    DrawableWallSurface._shouldAddToCost = false;
                    DrawableWallSurface._useStaticAddedWallCost = false;
                }

                if (Placeable.name == "POD_SpawnBasket_Vanilla")
                {
                    if (DefaultVariantPrefab.GetComponent<LevelEditorSpawnBasketCostOverride>())
                        UnityEngine.Object.Destroy(DefaultVariantPrefab.GetComponent<LevelEditorSpawnBasketCostOverride>());
                }
            }
        }

        public void RemoveCostAndRotationForObject(PlaceableObjectData Owner, bool RemoveCost = true, bool RemoveRotation = true)
        {
            foreach (var Variant in Owner.objectVariants)
            {
                //var TrueOwner = Variant.Prefab.GetComponent<LevelEditorPlaceableObject>().ObjectDataOwner;
                var CostHandler = Owner.costHandler; //TrueOwner.GetCostHandler();
                var RotationHandler = Owner.RotationHandler;
                if (CostHandler != null && RemoveCost)
                {
                    CostHandler._baseCost = -1;
                    CostHandler._additiveBaseCost = -1;
                    if (CostHandler._useStock) // resets every mapload unlike .UseStock
                    {
                        CostHandler._stockCountAllowed = 9000; // insurance
                        CostHandler._useStock = false; // disables using the stock
                        if (CostHandler.CMSData != null)
                        { 
                            CostHandler.CMSData.Stock = -1; // this disables the icon                         
                        }
                    }
                    
                    var prefab_comp = Variant.Prefab.GetComponent<LevelEditorCostOverrideFirstFree>();
                    if (prefab_comp)
                    { 
                        UnityEngine.Object.Destroy(prefab_comp); // still stays on the first pre-placed startline
                        //Main.Instance.Log.LogMessage("destroyed FirstFree for: " + TrueOwner.name + " v: " + Variant.name);
                    }
                    /*var prefab_comp2 = Variant.Prefab.GetComponent<LevelEditorScaleParameter>();
                    if (prefab_comp2) // wait nvm, MT devs were too lazy to edit prefabs this time
                    {
                        UnityEngine.Object.Destroy(prefab_comp2);
                        Main.Instance.Log.LogMessage("destroyed LevelEditorScaleParameter for: " + TrueOwner.name + " v: " + Variant.name);
                    }*/

                    if (CostHandler._firstFree)
                    {
                        CostHandler._firstFree = false;
                        if (CostHandler.CMSData != null)
                        {
                            CostHandler.CMSData.FirstFree = false;
                        }
                    }

                    if (CostHandler.CMSData != null)
                    {
                        CostHandler.CMSData.Settings.IsOverlappingEnabled = !FraggleExpansionData.CanClipObjects;
                    }
                }

                if (RotationHandler != null && RemoveRotation)
                {
                    RotationHandler.xAxisIsLocked = false;
                    RotationHandler.yAxisIsLocked = false;
                    RotationHandler.zAxisIsLocked = false;
                }
            }
        }

        public void AddAllObjectsToCurrentList()
        {
            foreach (var el in AssetRegistry.Instance._registry.Keys) // can't go by GUID values cuz can't Ref error
            {
                AddObjectToCurrentList(el, LevelEditorPlaceableObject.Category.Advanced, 0, itemID);
                itemID++;
            }
        }

        public void AddObjectToCurrentList(string AssetRegistryName, LevelEditorPlaceableObject.Category Category = LevelEditorPlaceableObject.Category.Advanced, int DefaultVariantIndex = 0, int ID = 0)
        {
            AddressableLoadableAsset Loadable = AssetRegistry.Instance.LoadAsset(AssetRegistryName);
            if (!Loadable.Asset) return; // missing data
            PlaceableObjectData Owner = Loadable.Asset.Cast<PlaceableVariant_Base>().Owner;
            var CurrentObjectTreeList = ThemeManager.CurrentThemeData.ObjectList.m_TreeElements;
            var CurrentObjectList = LevelEditorObjectList.CurrentObjects.Cast<Il2CppSystem.Collections.Generic.List<PlaceableObjectData>>();
            if (!Owner) { Log.LogMessage("asset " + AssetRegistryName + " has no owner data"); return; }
            if (HasCarouselDataForObject(Owner)) { /*Log.LogMessage("object " + AssetRegistryName + " is already in the list " +Owner.name);*/ return; }
            Loadable.LoadBlocking(); // does the thing, but categories now carry over to standard objects
            if (Owner.name == "POD_FanPlatform_OFF_Vanilla" || Owner.name == "POD_FanPlatform_ON_Vanilla") { return; } // blacklist // these guys somehow make digital maps softlock
            Owner.objectNameKey = Owner.name; // technical names for added objects
            VariantTreeElement VariantElement = new VariantTreeElement(Owner.name, 1, ID);
            //Owner.defaultVariant = Owner.objectVariants[DefaultVariantIndex];
            VariantElement.Variant = Owner.defaultVariant; //Owner.objectVariants[DefaultVariantIndex];
                
            // variant choice in the wheel:
            bool attached = false;
            string simpleName = Owner.name
                                .Replace("POD_Inflatable_Vanilla_Wall_beta", "Inflatable")
                                .Replace("POD_Drawable_Edge_Plain_Vanilla", "Curve")
                                .Replace("_Retro", "")
                                .Replace("_Vanilla", "")
                                .Replace("POD_Floor_Vinyl_Matt_2_2_Double_Height", "POD_Floor_Soft")
                                .Replace("POD_Floor_Hard_Plastic", "POD_Floor_Soft")
                                .Replace("Drawable_Ramp", "Ramp")
                                .Replace("POD_Drawable_Edge_", "")
                                .Replace("POD_Curve", "Curve")
                                .Replace("FloorStart", "Floor_Start")
                                .Replace("FloorEnd", "Floor_End")
                                .Replace("_Survival", "")
                                .Replace("_Revised", "")
                                .Replace("_1", "")
                                .Replace("_V1", "")
                                .Replace("_V2", "")
                                .Replace("Single", "")
                                .Replace("Double", "")
                                .Replace("_unification", "")
                                .Replace("_Updated", "")
                                .Replace("_Normal", "")
                                .Replace("_Short", "")
                                .Replace("_Vinyl", "")
                                .Replace("POD_Special_Goo_Slide", "Goop")
                                .Replace("SpinDoor", "Spin_Door")
                                .Replace("_Moderate", "")
                                .Replace("_Steep", "")
                                .Replace("_Strong", "")
                                .Replace("_Gentle", "")
                                .Replace("_Flat", "")
                                .Replace("SnowVanilla", "Snow")
                                .Replace("SnowRetro", "Snow")
                                .Replace("_Left", "")
                                .Replace("_Right", "")
                                .Replace("_Mid", "")
                                .Replace("_Hard", "")
                                .Replace("_Ramp_Soft", "_Ramp")
                                .Replace("_Edge", "")
                                .Replace("_Feature", "")
                                .Replace("POD_Wheel", "")
                                .Replace("_Common", "")
                                .Replace("_ON", "")
                                .Replace("_OFF", "")
                                .Replace("Multi", "")
                                .Replace("e_Post_End", "")
                                .Replace("POD_Cube", "POD_Floor_Soft")
                                .Replace("POD_Ramp", "POD_Drawable_Ramp")
                                .Replace("_Future", "");
            for (int i = CurrentObjectTreeList.Count-1; i >= 0 ; i--)
            {
                var CarouselItem = CurrentObjectTreeList[i];
                if (CarouselItem.Variant != null && CarouselItem.Variant.Owner.name.Contains(simpleName))
                {
                    if (CarouselItem.Variant.Owner.category != Owner.category)
                    {
                        Owner.category = CarouselItem.Variant.Owner.category;
                    }
                    if (CarouselItem.depth == 0)
                    {
                        itemID++;
                        CurrentObjectTreeList.Insert(i, new VariantTreeElement("Folder for " + Owner.name, 0, itemID));
                        CarouselItem.depth = 1;
                        i++; // our folder made it bigger
                        //Log.LogMessage("adding " + Owner.name + " ( " + simpleName + " ) to a new list " + i + " w: " + CarouselItem.name);
                    }
                    else
                    {
                        //Log.LogMessage("adding " + Owner.name + " ( " + simpleName + " ) to a list " + i + " of " + CarouselItem.children.Count + " w: " + CarouselItem.name);
                    }
                    CurrentObjectTreeList.Insert(i + 1, VariantElement); // place after it
                    attached = true;
                    break;
                }
            }
            if (!attached)
            {
                VariantElement.depth = 0;
                CurrentObjectTreeList.Add(VariantElement);
                //Log.LogMessage("adding " + Owner.name + " ( " + simpleName + " ) to the wheel root");
                if (Owner.category == LevelEditorPlaceableObject.Category.Hidden) { Owner.category = Category; Log.LogMessage("Object unHidden: " + Owner.name); }
            }
            if (!CurrentObjectList.Contains(Owner)) CurrentObjectList.Add(Owner); // active POD DB
            //else Log.LogMessage("already in the ObjectList DB: " + Owner.name);
        }
        public bool HasCarouselDataForObject(PlaceableObjectData Data)
        {
            foreach (var CarouselItem in ThemeManager.CurrentThemeData.ObjectList.m_TreeElements)
            {
                if (CarouselItem.Variant != null && CarouselItem.Variant.Owner == Data) // different names in PODs & carousel m_Name sometimes, also ref compare is faster
                    return true;
            }
            return false;
        }

        /*public void Dump_loaded_item_pod_names()
        {
            LevelEditorObjectList CurrentLevelEditorObjectList = ThemeManager.CurrentThemeData.ObjectList;
            foreach (var CarouselItem in CurrentLevelEditorObjectList.m_TreeElements)
            {
                if (CarouselItem.m_Children != null)
                {
                    Log.LogMessage("El w children: " + CarouselItem.m_Name);
                    foreach (var Child in CarouselItem.m_Children)
                    {
                        if (Child.m_Children != null)
                        {
                            Log.LogMessage("Child w children: " + Child.m_Name);
                            foreach (var subChild in Child.m_Children)
                            {
                                if (subChild.m_Children == null)
                                {
                                    Log.LogMessage("SubChild: " + subChild.m_Name + ", oN" + (subChild.Cast<VariantTreeElement>().Variant != null ? subChild.Cast<VariantTreeElement>().Variant.Owner.name : "[no variant]") + " d: " + subChild.depth + " i: " + subChild.Index + " si: " + subChild.SubIndex);
                                }
                                else
                                {
                                    Log.LogMessage("SubChild has children too" + subChild.m_Name);
                                }
                            }
                        }
                        else
                        {
                            Log.LogMessage("Child: " + Child.m_Name + ", oN" + (Child.Cast<VariantTreeElement>().Variant != null ? Child.Cast<VariantTreeElement>().Variant.Owner.name : "[no variant]") + " d: " +Child.depth + " i: " + Child.Index + " si: " + Child.SubIndex);
                        }
                    }
                }
                else
                {
                    Log.LogMessage("El: " + CarouselItem.m_Name + ", oN" + (CarouselItem.Cast<VariantTreeElement>().Variant !=null ? CarouselItem.Cast<VariantTreeElement>().Variant.Owner.name : "[no variant]") + " d: " + CarouselItem.depth + " i: " + CarouselItem.Index + " si: " + CarouselItem.SubIndex);
                }
            }
        }*/
    }
}


