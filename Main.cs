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
using static Il2CppSystem.Linq.Expressions.Interpreter.CastInstruction.CastInstructionNoT;
using System.Runtime.Serialization.Json;
using System.Linq;
using System.Text;
using static UnityEngine.AddressableAssets.Utility.SerializationUtilities;
using static Wushu.Framework.DebugDrawThreeJs;

namespace FraggleExpansion
{
    [BepInPlugin("FraggleExpansion", "Creative Expansion Pack CE", "47.0")]
    public class Main : BepInEx.Unity.IL2CPP.BasePlugin
    {
        public Harmony _Harmony = new Harmony("com.simp.fraggleexpansion");
        public static Main Instance;
        PlaceableObjectData BetaStart, DigitalStart, ClassicStart, SurvivalStart, SurvivalStartPoint, BetaEnd, DigitalEnd, ClassicEnd;
        //public SlimeGamemodesManager _SlimeGamemodeManager;

        public override void Load()
        {
            Instance = this;
            _Harmony.PatchAll(typeof(MainInit));
        }
        public void LateLoad()
        {
            //Log.LogMessage("Creative Expansion Pack CE 2.4");

            _ = new PropertiesReader();
            _ = new myXml();
            //_SlimeGamemodeManager = new SlimeGamemodesManager();

            // Requirement to Intialize Creative Expansion Pack
            //_Harmony.PatchAll(typeof(Requirements)); // lithum bs

            // Within Creative Patches
            _Harmony.PatchAll(typeof(MainFeaturePatches));
            _Harmony.PatchAll(typeof(FeaturesPatches));
            /*if (FraggleExpansionData.UseMainSkinInExploreState) // breaks frequently
            {
                _Harmony.PatchAll(typeof(FeaturesPatchesSkin));
            }*/
            _Harmony.PatchAll(typeof(BypassesPatches));

            // UI Stuff
            _Harmony.PatchAll(typeof(ReticleUI));

            // Misc.
            _Harmony.PatchAll(typeof(BugFixes));
            //_Harmony.PatchAll(typeof(MiscPatches));
            //Log.LogMessage("RUN: Patches DONE");
        }

        public bool Setup_done = false;
        //public bool Buttons_done = false;
        /*public void SetUp()
        {
            if (!Setup_done)
            {
                Setup_done = true;
                //PropertiesReader.Instance.InitializeData(); // re-read config every map load
                ManageAllCurrentObjects();
            }
        }*/

        public void SetupStartLines() // else it will softlock on publish test with a custom start-line
        {
            if (!ThemeManager.CurrentStartGantries.Contains(ClassicStart)) { ThemeManager.CurrentStartGantries.Add(ClassicStart); }
            if (!ThemeManager.CurrentStartGantries.Contains(DigitalStart)) { ThemeManager.CurrentStartGantries.Add(DigitalStart); }
            if (!ThemeManager.CurrentStartGantries.Contains(BetaStart)) { ThemeManager.CurrentStartGantries.Add(BetaStart); }
            if (!ThemeManager.CurrentStartGantries.Contains(SurvivalStart)) { ThemeManager.CurrentStartGantries.Add(SurvivalStart); }
            if (!ThemeManager.CurrentStartGantries.Contains(SurvivalStartPoint)) { ThemeManager.CurrentStartGantries.Add(SurvivalStartPoint); }
        }

        /*public int CountStartLines() // thx MT for implementing yet another CEP feature
        {
            int nClassic, nDigital, nBeta, nSurvival, nSurvivalP;
            if (ClassicStart) { nClassic = LevelEditorManager.Instance.CostManager.GetCount(ClassicStart); } else { nClassic = 0; }
            if (DigitalStart) { nDigital = LevelEditorManager.Instance.CostManager.GetCount(DigitalStart); } else { nDigital = 0; }
            if (BetaStart) { nBeta = LevelEditorManager.Instance.CostManager.GetCount(BetaStart); } else { nBeta = 0; }
            if (SurvivalStart) { nSurvival = LevelEditorManager.Instance.CostManager.GetCount(SurvivalStart); } else { nSurvival = 0; }
            if (SurvivalStartPoint) { nSurvivalP = LevelEditorManager.Instance.CostManager.GetCount(SurvivalStartPoint); } else { nSurvivalP = 0; }
            //Log.LogMessage("found starts: " + (nClassic + nDigital + nBeta + nSurvival + nSurvivalP));
            if (nClassic > 0) { ThemeManager.CurrentStartGantry = ClassicStart; }
            else if (nDigital > 0) { ThemeManager.CurrentStartGantry = DigitalStart; }
            else if (nBeta > 0) { ThemeManager.CurrentStartGantry = BetaStart; }
            else if (nSurvival > 0) { ThemeManager.CurrentStartGantry = SurvivalStart; }
            else if (nSurvivalP > 0) { ThemeManager.CurrentStartGantry = SurvivalStartPoint; }
            return nClassic + nDigital + nBeta + nSurvival + nSurvivalP;
        }*/

        /*public void CountEndLines()
        {
            if (ClassicEnd && LevelEditorManager.Instance.CostManager.GetCount(ClassicEnd) > 0) { ThemeManager.CurrentFinishGantry = ClassicEnd; }
            if (DigitalEnd && LevelEditorManager.Instance.CostManager.GetCount(DigitalEnd) > 0) { ThemeManager.CurrentFinishGantry = DigitalEnd; }
            if (BetaEnd && LevelEditorManager.Instance.CostManager.GetCount(BetaEnd) > 0) { ThemeManager.CurrentFinishGantry = BetaEnd; }
            //Log.LogMessage("found ends: " + nClassic + " " + nDigital + " " + nBeta);
        }*/

        /*public void ManageAllCurrentObjects()
        {
            for (int i = LevelEditorObjectList.CurrentObjects.Cast<Il2CppSystem.Collections.Generic.List<PlaceableObjectData>>().Count - 1; i >= 0; i--)
                Manage_POD(LevelEditorObjectList.CurrentObjects[i]);
        }*/

        public void Manage_POD(PlaceableObjectData Owner)
        {
            foreach (var Variant in Owner.objectVariants)
            {
                var CostHandler = Owner.costHandler; //Owner.GetCostHandler();
                if (CostHandler != null)
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
                    if (CostHandler.IsHiddenFromCarousel && FraggleExpansionData.AddAllObjects) // hidden items
                    {
                        if (CostHandler.CMSData != null)
                        {
                            CostHandler.CMSData.HideFromCarousel = false;                        
                        }
                        Log.LogMessage("Unhide item: " + Owner.name + " cat: " + Owner.category);
                    }

                    if (FraggleExpansionData.RemoveDefaultScalingFeature)
                    {
                        var prefab_comp2 = Variant.Prefab.GetComponent<LevelEditorScaleParameter>();
                        if (prefab_comp2) // wait nvm, MT devs were too lazy to edit prefabs this time // but now they've started doing it...
                        {
                            UnityEngine.Object.Destroy(prefab_comp2);
                            //Main.Instance.Log.LogMessage("destroyed LevelEditorScaleParameter for: " + Owner.name + " v: " + Variant.name);
                        }
                    }
                    else // not really needed, cuz the game adds it itself if that setting bolean is true
                    {
                        var prefab_comp2 = Variant.Prefab.GetComponent<LevelEditorScaleParameter>();
                        if (!prefab_comp2 && !Owner.name.Contains("Inflatable") && !Owner.name.Contains("Fillet")) // Inflatable // yes, we can scale walls but we can't exit that menu for some reason... // we can also scale fillets but that won't save
                        {
                            Variant.Prefab.AddComponent<LevelEditorScaleParameter>();
                            Variant.Prefab.GetComponent<LevelEditorPlaceableObject>().hasParameterComponents = true; // for those with no params
                        }
                    }

                    if (CostHandler._firstFree) // not needed anymore actually
                    {
                        CostHandler._firstFree = false;
                        if (CostHandler.CMSData != null)
                        {
                            CostHandler.CMSData.FirstFree = false;
                        }
                    }

                    if (CostHandler.CMSData != null && CostHandler.CMSData.Settings != null)
                    {
                        if (!Owner.name.Contains("Inflatable") && !Owner.name.Contains("Fillet"))
                        {
                            CostHandler.CMSData.Settings.IsScalingEnabled = true;
                        }
                        CostHandler.CMSData.Settings.IsScalingUniform = false;
                        CostHandler.CMSData.Settings.MaximumScale = 20.0f; // doesn't work?? // where does it even take its limit from...?
                        if (CostHandler._genericCostsCMSData != null && CostHandler._genericCostsCMSData.Settings != null) CostHandler._genericCostsCMSData.Settings.MaximumScale = 20.0f; // not this either
                        //CostHandler.CMSData.Settings.ScaleIgnoredAxis = []; // not needed
                        CostHandler.CMSData.Settings.IsOverlappingEnabled = true; // remove stupid icon over items
                    }
                }

                var RotationHandler = Owner.RotationHandler;
                if (RotationHandler != null)
                {
                    RotationHandler.xAxisIsLocked = false;
                    RotationHandler.yAxisIsLocked = false;
                    RotationHandler.zAxisIsLocked = false;
                }

                Manage_GameObject(Variant.Prefab);
            }

            // Single Varianted:
            if (Owner.name == "POD_Rule_FloorStart_Vanilla")
            {
                BetaStart = Owner;
                Owner.matchingCriteria = FraggleCriteria.Criteria.MinimumStartPoints;
            }
            else if (Owner.name == "POD_Rule_Floor_Start_Retro")
            {
                DigitalStart = Owner;
            }
            else if (Owner.name == "POD_Rule_Floor_Start_Revised_Vanilla")
            {
                ClassicStart = Owner;
            }
            else if (Owner.name == "POD_FloorStart_Survival_SpawnPoint_Vanilla")
            {
                SurvivalStartPoint = Owner;
            }
            else if (Owner.name == "POD_Rule_Floor_Start_Survival")
            {
                SurvivalStart = Owner;
                Owner.defaultVariant.Prefab.GetComponent<LevelEditorPlaceableObject>().EditorOnlyRenderers = null; // this makes it visible
                Owner.defaultVariant.Prefab.GetComponent<LevelEditorPlaceableObject>().collidersToDisable = null; // this makes it touchable
                Owner.matchingCriteria = FraggleCriteria.Criteria.MinimumStartPoints;
                //Placeable.defaultVariant.Prefab.GetComponent<LevelEditorPlaceableObject>().ParameterTypes = LevelEditorParametersManager.LegacyParameterTypes.None;
            }
            else if (Owner.name == "POD_UI_Trigger_Volume") // Owner.name == "POD_Drawable_Killzone_Common" || Owner.name == "POD_Drawable_Killzone_Gauntlet_Common" - can't make em visible easily
            {
                Owner.defaultVariant.Prefab.GetComponent<LevelEditorPlaceableObject>().EditorOnlyRenderers = null; // this makes it visible
            }
            else if (Owner.name == "POD_Rule_FloorEnd_Vanilla")
            {
                BetaEnd = Owner;
                Owner.matchingCriteria = FraggleCriteria.Criteria.FinishLine;
            }
            else if (Owner.name == "POD_Rule_Floor_End_Retro")
            {
                DigitalEnd = Owner;
            }
            else if (Owner.name == "POD_Rule_Floor_End_Revised_Vanilla")
            {
                ClassicEnd = Owner;
            }
            /*else if (Owner.name == "POD_Feature_Bubble_Vanilla")
            {
                //LevelEditorBubbleParameters.StepBubblePointsAwarded = 1;
                //LevelEditorBubbleParameters.MaxBubblePointsAwarded = 10;
                //LevelEditorBubbleParameters.MinBubblePointsAwarded = -10;
                //Owner.defaultVariant.Prefab.GetComponent<LevelEditorBubbleParameters>().StepBubblePointsAwarded = 0;
            }*/
            /*else if (Owner.name == "POD_SeeSaw_Vanilla")
            {
                Owner.category = LevelEditorPlaceableObject.Category.MovingSurfaces;
            }
            else if (Placeable.name == "POD_Special_Goo_Slide")
            {
                Placeable.category = LevelEditorPlaceableObject.Category.Platforms;
            }*/
            /*else if ((Placeable.name == "POD_Floating_Cannon_Revised_Vanilla" && ThemeManager.CurrentThemeData.ID != "THEME_RETRO") || (Placeable.name == "POD_Floating_Cannon_Retro" && ThemeManager.CurrentThemeData.ID == "THEME_RETRO"))
            {
                Prefab.AddComponent<LevelEditorReceiver>();
                Prefab.GetComponentInChildren<CannonActiveStateEventResponders>()._eventResponderNameKey = "wle_event_responder_toggle_on_off";
            }*/ // it makes maps not load
        }

        public void Manage_GameObject(GameObject Prefab) // Prefab
        {
            var prefab_comp_ff = Prefab.GetComponent<LevelEditorCostOverrideFirstFree>();
            if (prefab_comp_ff)
            {
                UnityEngine.Object.Destroy(prefab_comp_ff); // still stays on the first pre-placed startline
            }
            if (FraggleExpansionData.RemoveBuoyancy)
            {
                var Buoyancy = Prefab.GetComponent<LevelEditorGenericBuoyancy>();
                if (Buoyancy) UnityEngine.Object.Destroy(Buoyancy); // no more floating up and down
            }
            var Drawable = Prefab.GetComponent<LevelEditorDrawableData>();
            if (Drawable)
            {
                var POD_Name = Prefab.GetComponent<LevelEditorPlaceableObject>().ObjectDataOwner.name;
                if (POD_Name == "POD_Floor_Hard_Plastic" || POD_Name == "POD_Floor_Vinyl_Conveyor")
                {
                    UnityEngine.Object.Destroy(Drawable);
                    return;
                }
                else if (POD_Name == "POD_UI_Trigger_Volume")
                {
                    /*var prefab_comp = Prefab.GetComponent<LevelEditorTransmitter>();
                    if (prefab_comp)
                    {
                        UnityEngine.Object.Destroy(prefab_comp);
                    }*/
                    var prefab_comp2 = Prefab.GetComponent<LevelEditorTriggerableParameter>();
                    if (prefab_comp2)
                    {
                        UnityEngine.Object.Destroy(prefab_comp2); // MT deleted\renamed settings & corrupted maps bruh
                    }
                    var prefab_comp3 = Prefab.GetComponent<LevelEditorCooldownParameter>();
                    if (prefab_comp3)
                    {
                        UnityEngine.Object.Destroy(prefab_comp3);
                    }
                }
                else if (Prefab.GetComponent<LevelEditorCheckpointFloorData>() && POD_Name != "POD_Drawable_CheckpointTriggerZone_Common" && POD_Name != "POD_FloorStart_SpawnPoint_Pack_Vanilla")
                {
                    Drawable._painterMaxSize = new Vector3(10000, 10000, 10000);
                    Drawable._canBePainterDrawn = true;
                    Drawable.FloorType = LevelEditorDrawableData.DrawableSemantic.FloorObject;
                    Drawable._restrictedDrawingAxis = LevelEditorDrawableData.DrawRestrictedAxis.Up;
                    //UnityEngine.Object.Destroy(Prefab.GetComponent<LevelEditorFloorScaleParameter>()); // why ?
                    //Prefab.GetComponent<LevelEditorPlaceableObject>().hasParameterComponents = false;
                    return;
                }
                /*else // this allows to create plain edges with ghosting sides, also making barriers and checkpoints taller on date but not in reality
                {
                    Drawable._restrictedDrawingAxis = LevelEditorDrawableData.DrawRestrictedAxis.None;
                }*/
                Drawable._painterMaxSize = new Vector3(100000, 100000, 100000);
                //Drawable.DrawableDepthMaxIncrements = 100000; // GhostBlocks - let's do it another way now, not like this
            }

            /*var prefab_comp_ws = Prefab.GetComponent<LevelEditorDrawablePremadeWallSurface>();
            if (prefab_comp_ws)
            {
                prefab_comp_ws._shouldAddToCost = false;
                prefab_comp_ws._useStaticAddedWallCost = false;
            }*/
        }

        public int itemID = 2000;
        public void AddCustomObjectsToCurrentList()
        {
            itemID = 2000;
            foreach (string AssetRegistryName in FraggleExpansionData.AddObjectData)
            {
                AddressableLoadableAsset Loadable = AssetRegistry.Instance.LoadAsset(AssetRegistryName);
                if (Loadable.Asset == null)
                {
                    //Loadable.Unload(); // what's the difference tho?
                    Loadable.Dispose();
                    return;
                }
                PlaceableObjectData Owner = Loadable.Asset.Cast<PlaceableVariant_Base>().Owner;
                AddObjectToCurrentList(Owner, LevelEditorPlaceableObject.Category.Advanced, 0, itemID);
                itemID++;
            }
        }
        public void AddAllObjectsToCurrentList()
        {
            itemID = 2000;
            //foreach (var el in AssetRegistry.Instance._registry.Keys) // can't go by GUID values cuz can't Ref error
            //Log.LogMessage("AllValidPODs size: " + AllValidPODs.Count);
            foreach (var el in AllValidPODs)
            {
                AddObjectToCurrentList(el, LevelEditorPlaceableObject.Category.Advanced, 0, itemID);
                itemID++;
            }
        }

        public void AddObjectToCurrentList(PlaceableObjectData Owner, LevelEditorPlaceableObject.Category Category = LevelEditorPlaceableObject.Category.Advanced, int DefaultVariantIndex = 0, int ID = 0)
        {
            try
            {
                /*AddressableLoadableAsset Loadable = AssetRegistry.Instance.LoadAsset(AssetRegistryName);
                if (Loadable.Asset == null)
                {
                    //Loadable.Unload(); // what's the difference tho?
                    Loadable.Dispose();
                    return;
                }
                PlaceableObjectData Owner = Loadable.Asset.Cast<PlaceableVariant_Base>().Owner;*/

                var CurrentObjectTreeList = ThemeManager.CurrentThemeData.ObjectList.m_TreeElements;
                if (Owner == null) { /*Log.LogMessage("asset " + AssetRegistryName + " has no owner data");*/ return; }
                if (HasCarouselDataForObject(Owner)) { /*Log.LogMessage("object " + AssetRegistryName + " is already in the list " +Owner.name);*/ return; }
                //Loadable.LoadBlocking(); // makes objects which usually don't get loaded load, but categories now carry over to standard objects - how to check if we need to call it? - haven't found a way.... // ok, we don't need this anymore after Preprocessing f()
                if (Owner.name == "POD_FanPlatform_OFF_Vanilla" || Owner.name == "POD_FanPlatform_ON_Vanilla" || Owner.name == "POD_Floor_Fillet") { return; } // blacklist // these guys somehow make digital maps softlock // and the fillet can't be placed
                Owner.objectNameKey = Owner.name; // technical names for added objects
                VariantTreeElement VariantElement = new VariantTreeElement(Owner.name, 1, ID);
                //Owner.defaultVariant = Owner.objectVariants[DefaultVariantIndex];
                VariantElement.Variant = Owner.defaultVariant; //Owner.objectVariants[DefaultVariantIndex];

                // variant choice in the wheel:
                bool attached = false;
                string simpleName = Owner.name
                                    .Replace("POD_Inflatable_Vanilla_Wall_beta", "Inflatable")
                                    .Replace("POD_Drawable_Edge_Plain_Vanilla", "Curve")
                                    .Replace("POD_FloorStart_Survival_SpawnPoint_Vanilla", "Start")
                                    .Replace("POD_UI_Trigger_Volume", "POD_Trigger_Zone")
                                    //.Replace("GoopRipple", "Ripple")
                                    .Replace("POD_Special_Goo_Slide", "Trench")
                                    .Replace("_Retro", "")
                                    .Replace("_Vanilla", "")
                                    .Replace("POD_Floor_Vinyl_Matt_2_2_Double_Height", "POD_Floor_Soft")
                                    .Replace("POD_Floor_Hard_Plastic", "POD_Floor_Soft")
                                    .Replace("Drawable_Ramp", "Ramp")
                                    .Replace("POD_Hard_Wedge", "Primitive_Triangle")
                                    .Replace("POD_Drawable_Edge_", "")
                                    .Replace("POD_Curve", "Curve")
                                    .Replace("POD_Rule_", "")
                                    .Replace("FloorStart", "Start")
                                    .Replace("Floor_Start", "Start")
                                    .Replace("FloorEnd", "End")
                                    .Replace("Floor_End", "End")
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
                                    .Replace("_Points", "")
                                    .Replace("_Race", "")
                                    .Replace("_ON", "")
                                    .Replace("_OFF", "")
                                    .Replace("Multi", "")
                                    .Replace("e_Post_End", "")
                                    .Replace("POD_Cube", "POD_Floor_Soft")
                                    .Replace("POD_Ramp", "POD_Drawable_Ramp")
                                    .Replace("_Gauntlet", "")
                                    .Replace("_Future", "");
                for (int i = CurrentObjectTreeList.Count - 1; i >= 0; i--)
                {
                    var CarouselItem = CurrentObjectTreeList[i];
                    if (CarouselItem.Variant != null && CarouselItem.Variant.Owner.name.Contains(simpleName) && CarouselItem.Variant.Owner.name != "POD_Wall_Inflate_Post_End" && CarouselItem.Variant.Owner.name != "POD_Drawable_CheckpointTriggerZone_Common" && CarouselItem.name != "PrimitiveSpheres") // MT added "PrimitiveSpheres" #87 folder wrong, facepalm
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
                            //Log.LogMessage("adding " + Owner.name + " ( " + simpleName + " ) to a new list " + (i-1) + " w: " + CarouselItem.Variant.Owner.name + " aka: " + CarouselItem.name + " category: " + Owner.category);
                        }
                        else
                        {
                            //Log.LogMessage("adding " + Owner.name + " ( " + simpleName + " ) to a list " + i + " w: " + CarouselItem.Variant.Owner.name + " aka: " + CarouselItem.name + " category: " + Owner.category);
                            //if (CarouselItem.parent.Cast<VariantTreeElement>().SelectedIndex != 0) CarouselItem.parent.Cast<VariantTreeElement>().SelectedIndex = 0; // Fans folder
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
                // active POD DB :
                var CurrentObjectList = LevelEditorObjectList.CurrentObjects; // lets work natively with this bs, maybe this is why things break
                if (CurrentObjectList.IndexOf(Owner) == -1) CurrentObjectList.Insert(CurrentObjectList.Cast<Il2CppSystem.Collections.Generic.List<PlaceableObjectData>>().Count, Owner);
                //else Log.LogMessage("already in the ObjectList POD DB: " + Owner.name);

                //Manage_POD(Owner); // already PreProcessed so no need anymore
            }
            catch (System.Exception e) { Log.LogMessage(e); }
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

        public List<PlaceableObjectData> AllValidPODs = new();
        public bool Preprocessed = false;
        public void Preproccess_POD_prefabs()
        {
            Preprocessed = true;
            try
            {
                if (!AssetRegistry.Instance.Initialised) AssetRegistry.Instance.Initialise(true); // what does this arg do?
                PlaceableObjectData LastOwner = null;
                foreach (var el in AssetRegistry.Instance._registry.Keys) // can't go by GUID values cuz can't Ref error
                {
                    AddressableLoadableAsset Loadable = AssetRegistry.Instance.LoadAsset(el);
                    Loadable.LoadBlocking();
                    if (Loadable.Asset == null) { Loadable.Dispose(); continue; } // missing data

                    PlaceableObjectData Owner = Loadable.Asset.Cast<PlaceableVariant_Base>().Owner;
                    if (Owner == null || LastOwner == Owner) continue;

                    LastOwner = Owner;
                    AllValidPODs.Add(Owner);
                    Manage_POD(Owner);
                }
            }
            catch (System.Exception e) { Log.LogMessage(e); }
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


