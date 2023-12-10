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
        
        public void SetUp()
        {
            PropertiesReader.Instance.InitializeData(); // re-read config every map load
            //if (FraggleExpansionData.BypassBounds) // this here does nothing
            //    LevelEditorManager.Instance.MapPlacementBounds = new Bounds(LevelEditorManager.Instance.MapPlacementBounds.center, new Vector3(100000, 100000, 100000));

            if (FraggleExpansionData.AddUnusedObjects)
            {
                //Log.LogMessage("Objects to add: " + FraggleExpansionData.AddObjectData.Length);
                int i = 5000;
                foreach (string sData in FraggleExpansionData.AddObjectData)
                {
                    //Log.LogMessage("Adding object: " + sData);
                    AddObjectToCurrentList(sData, LevelEditorPlaceableObject.Category.Advanced,0,i);
                    i++;
                }
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

                if (Placeable.name == "POD_Wheel_Maze_Revised_Common")
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

        public TreeElement test;

        public void AddObjectToCurrentList(string AssetRegistryName, LevelEditorPlaceableObject.Category Category = LevelEditorPlaceableObject.Category.Advanced, int DefaultVariantIndex = 0, int ID = 0)
        {
            try
            {
                AddressableLoadableAsset Loadable = AssetRegistry.Instance.LoadAsset(AssetRegistryName);
                if (!Loadable.Asset) // missing data
                { 
                    Loadable.LoadBlocking();
                    if (!Loadable.Asset) { Log.LogMessage("couldn't load asset for: " + AssetRegistryName); return; }
                    Log.LogMessage("Load asset succeess for: " + AssetRegistryName);
                }
                PlaceableObjectData Owner = Loadable.Asset.Cast<PlaceableVariant_Base>().Owner;
                LevelEditorObjectList CurrentLevelEditorObjectList = ThemeManager.CurrentThemeData.ObjectList;
                var CurrentObjectList = LevelEditorObjectList.CurrentObjects.Cast<Il2CppSystem.Collections.Generic.List<PlaceableObjectData>>();
                if (!Owner) { Log.LogMessage("asset " + AssetRegistryName + " has no owner data"); return; }
                if (HasCarouselDataForObject(Owner)) { /*Log.LogMessage("object " + AssetRegistryName + " is already in the list " +Owner.name);*/ return; }
                Loadable.LoadBlocking(); // does the thing, but categories now carry over to standard objects
                if (Owner.category == LevelEditorPlaceableObject.Category.Hidden) { Owner.category = Category; Log.LogMessage("Object unHidden: " + Owner.name); }
                Owner.objectDescriptionKey = Owner.name;
                VariantTreeElement VariantElement = new VariantTreeElement(Owner.name, 0, ID);
                //Owner.defaultVariant = Owner.objectVariants[DefaultVariantIndex];
                VariantElement.Variant = Owner.defaultVariant; //Owner.objectVariants[DefaultVariantIndex];
                
                // variant choice in the wheel:
                // ... in UE you can add sub-children fine but with code here it doesn't work, sadge
                /*bool attached = false;
                string simpleName = Owner.name.Replace("_Retro", "").Replace("_Vanilla", "").Replace("Drawable_", "").Replace("Rule_Floor_", "").Replace("Rule_Floor", "").Replace("_Revised", "").Replace("_1", "").Replace("_V1", "").Replace("_V2", "").Replace("Single", "").Replace("Double", "").Replace("_unification", "").Replace("_Updated", "").Replace("_Normal", "").Replace("_Short", "");
                for (int i = 0; i < ThemeManager.CurrentThemeData.ObjectList.m_TreeElements.Count; i += 1) //foreach (TreeElement CarouselItem in ThemeManager.CurrentThemeData.ObjectList.CarouselItems.children)
                {
                    var CarouselItem = ThemeManager.CurrentThemeData.ObjectList.m_TreeElements[i];
                    if (i != 0 && CarouselItem.m_Children != null)
                    {
                        foreach (var subChild in CarouselItem.m_Children)
                        {
                            if (subChild.m_Name.Contains(simpleName))
                            {
                                Log.LogMessage("adding "+Owner.name+" ( "+ simpleName + " ) to existing list " + i + " of "+ CarouselItem.m_Children.Count + " w: " + subChild.name);
                                attached = true;
                                VariantElement = new VariantTreeElement(Owner.name, 1, ID);
                                VariantElement.Variant = Owner.defaultVariant;
                                
                                Log.LogMessage("m_Children size Bbefore " + CarouselItem.m_Children.Count);
                                CarouselItem.m_Children.Add(VariantElement);
                                Log.LogMessage("m_Children size after " + CarouselItem.m_Children.Count);
                                break;
                            }
                        }
                    }
                    else if (CarouselItem.m_Name.Contains(simpleName))
                    {
                        Log.LogMessage("adding " + Owner.name + " to a new list "+ CarouselItem.SubIndex+" w: " + CarouselItem.m_Name);
                        attached = true;
                        VariantElementFolder.children = new Il2CppSystem.Collections.Generic.List<TreeElement>(2);
                        VariantElementFolder.children.Add(CarouselItem); // self
                        VariantElementFolder.children.Add(VariantElement);
                        CarouselItem.Remove(CarouselItem); // self
                        break;
                    }
                    if (attached) { break; }
                }
                if (!attached)
                {
                    Log.LogMessage("adding " + Owner.name + " to the wheel root");
                    ThemeManager.CurrentThemeData.ObjectList.m_TreeElements.Add(VariantElement);
                }*/
                
                //CurrentLevelEditorObjectList.CarouselItems.m_Children.Add(VariantElement); // same list as below but without smth
                CurrentLevelEditorObjectList.m_TreeElements.Add(VariantElement); // the menu
                if (!CurrentObjectList.Contains(Owner)) CurrentObjectList.Add(Owner); // POD DB
                //else Log.LogMessage("already in the ObjectList DB: " + Owner.name);
            }
            catch (System.Exception e) { Log.LogMessage(e); }
        }

        public bool HasCarouselDataForObject(PlaceableObjectData Data)
        {
            LevelEditorObjectList CurrentLevelEditorObjectList = ThemeManager.CurrentThemeData.ObjectList;
            foreach (var CarouselItem in CurrentLevelEditorObjectList.CarouselItems.children)
            {
                if (CarouselItem.hasChildren) 
                {
                    foreach (var subChild in CarouselItem.m_Children)
                    {
                        if (subChild.Cast<VariantTreeElement>().Variant.Owner.name == Data.name)
                            return true;
                    }
                }
                else if (CarouselItem.Cast<VariantTreeElement>().Variant.Owner.name == Data.name) // different names in carousel sometimes
                    return true;
            }

            return false;
        }

        public bool FindSimilarCarouselDataForObject(PlaceableObjectData Data)
        {
            LevelEditorObjectList CurrentLevelEditorObjectList = ThemeManager.CurrentThemeData.ObjectList;
            foreach (var CarouselItem in CurrentLevelEditorObjectList.CarouselItems.children)
            {
                if (CarouselItem.hasChildren)
                {
                    foreach (var subChild in CarouselItem.m_Children)
                    {
                        if (subChild.m_Name == Data.name)
                            return true;
                    }
                }
                else if (CarouselItem.m_Name == Data.name)
                    return true;
            }

            return false;
        }
    }
}


