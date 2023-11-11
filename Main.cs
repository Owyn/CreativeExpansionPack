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

namespace FraggleExpansion
{
    [BepInPlugin("FraggleExpansion", "Creative Expansion Pack CE", "2.4")]
    public class Main : BepInEx.Unity.IL2CPP.BasePlugin
    {
        public Harmony _Harmony = new Harmony("com.simp.fraggleexpansion");
        public static Main Instance;
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
                foreach (string sData in FraggleExpansionData.AddObjectData)
                {
                    //Log.LogMessage("Adding object: " + sData);
                    AddObjectToCurrentList(sData, LevelEditorPlaceableObject.Category.Advanced, 0, 0);
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

                /*if (Placeable.name == "POD_Rule_Floor_Start_Survival")
                {
                    Placeable.objectNameKey = "wle_item_holographicstartname";
                    Placeable.objectDescriptionKey = "wle_item_holographicstartdesc";

                    Placeable.defaultVariant.Prefab.GetComponent<LevelEditorPlaceableObject>().ParameterTypes = LevelEditorParametersManager.LegacyParameterTypes.None;
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

        public void ManageCostRotationStockForAllObjects(bool RemoveCostAndStock, bool RemoveRotation)
        {
            foreach (var Placeable in LevelEditorObjectList.CurrentObjects.Cast<Il2CppSystem.Collections.Generic.List<PlaceableObjectData>>())
            {
                RemoveCostAndRotationForObject(Placeable, RemoveCostAndStock, RemoveRotation);

                if (!RemoveCostAndStock) return;

                var DefaultVariantPrefab = Placeable.defaultVariant.Prefab;

                if (Placeable.name == "POD_Wheel_Maze_Revised")
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
                var TrueOwner = Variant.Prefab.GetComponent<LevelEditorPlaceableObject>().ObjectDataOwner;
                var CostHandler = TrueOwner.GetCostHandler();
                var RotationHandler = TrueOwner.RotationHandler;
                if (CostHandler != null && RemoveCost)
                {
                    CostHandler._baseCost = -1;
                    CostHandler._additiveBaseCost = -1;
                    if (CostHandler._useStock) // resets every mapload unlike .UseStock
                    {
                        CostHandler._stockCountAllowed = 9000; // insurance
                        CostHandler._useStock = false; // disables using the stock
                        CostHandler.CMSData.Stock = -1; // this disables the icon
                    }
                    
                    var prefab_comp = Variant.Prefab.GetComponent<LevelEditorCostOverrideFirstFree>();
                    if (prefab_comp)
                    { 
                        UnityEngine.Object.Destroy(prefab_comp); // still stays on the first pre-placed startline
                        //Main.Instance.Log.LogMessage("destroyed FirstFree for: " + TrueOwner.name + " v: " + Variant.name);
                    }
                    if (CostHandler._firstFree)
                    {
                        CostHandler._firstFree = false;
                        CostHandler.CMSData.FirstFree = false;
                    }
                    CostHandler.CMSData.Settings.IsOverlappingEnabled = !FraggleExpansionData.CanClipObjects;
                }

                if (RotationHandler != null && RemoveRotation)
                {
                    RotationHandler.xAxisIsLocked = false;
                    RotationHandler.yAxisIsLocked = false;
                    RotationHandler.zAxisIsLocked = false;
                }
            }
        }

        public void AddObjectToCurrentList(string AssetRegistryName, LevelEditorPlaceableObject.Category Category = LevelEditorPlaceableObject.Category.Advanced, int DefaultVariantIndex = 0, int ID = 0)
        {
            try
            {
                AddressableLoadableAsset Loadable = AssetRegistry.Instance.LoadAsset(AssetRegistryName);
                PlaceableObjectData Owner = Loadable.Asset.Cast<PlaceableVariant_Base>().Owner;
                LevelEditorObjectList CurrentLevelEditorObjectList = ThemeManager.CurrentThemeData.ObjectList;
                var CurrentObjectList = LevelEditorObjectList.CurrentObjects.Cast<Il2CppSystem.Collections.Generic.List<PlaceableObjectData>>();
                if (Owner == null) return;
                if (CurrentObjectList.Contains(Owner) && HasCarouselDataForObject(Owner)) return;
                Owner.category = Category;
                VariantTreeElement VariantElement = new VariantTreeElement(Owner.name, 0, ID);
                Owner.defaultVariant = Owner.objectVariants[DefaultVariantIndex];
                VariantElement.Variant = Owner.objectVariants[DefaultVariantIndex];
                CurrentLevelEditorObjectList.CarouselItems.children.Add(VariantElement);
                CurrentLevelEditorObjectList.treeElements.Add(VariantElement);
                CurrentObjectList.Add(Owner);
            }
            catch { }
        }

        public bool HasCarouselDataForObject(PlaceableObjectData Data)
        {
            LevelEditorObjectList CurrentLevelEditorObjectList = ThemeManager.CurrentThemeData.ObjectList;
            foreach (var CarouselItem in CurrentLevelEditorObjectList.CarouselItems.children)
            {
                if (CarouselItem.Cast<VariantTreeElement>().Variant.Owner == Data)
                    return true;
            }

            return false;
        }
    }
}


