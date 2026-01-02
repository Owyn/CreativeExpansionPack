using FG.Common;
using FraggleExpansion.Patches.Creative;
using HarmonyLib;

namespace FraggleExpansion.Patches.Reticle
{
    public class ReticleUI
    {
        public static string level_size = "Objects"; //"";

        #region Budget Bar Modifications
        [HarmonyPatch(typeof(LevelEditorResourceBarViewModel), nameof(LevelEditorResourceBarViewModel.TotalPointsText), MethodType.Getter), HarmonyPrefix]
        public static bool ResourceBarTotalUsablePoints(out string __result)
        {
            __result = level_size; //+" / 1.5MB"; //  ∞ 
            return false;
        }

        /*[HarmonyPatch(typeof(LevelEditorResourceBarViewModel), nameof(LevelEditorResourceBarViewModel.BuildPointsUsedText), MethodType.Getter), HarmonyPrefix]
        public static bool ResourceBarUsedBuildPoints(out string __result)
        {
            __result = "NA  ";
            return !FraggleExpansionData.RemoveCostAndStock;
        }
        */
        #endregion

        // !LevelEditorManager.Instance.UI._radialDefinition.RadialDefinitions[2].IsToggleOn(); // what to use this button for tho?
        // catch overlap setting icon being clicked

        //void LevelEditor_RadialMenuItemViewModel::OnButtonClick()
        //- __instance: Prime_UI_LE_RadialMenuItemVertical_Canvas (Right) (LevelEditor_RadialMenuItemViewModel)

        #region Text Patches

        [HarmonyPatch(typeof(LevelEditorCarrouselItemViewModel), nameof(LevelEditorCarrouselItemViewModel.UpdateObjectOverlapping)), HarmonyPrefix]
        public static bool ItemOverlapDisplay(LevelEditorCarrouselItemViewModel __instance)
        {
            __instance.CanObjectOverlap = false; // remove icon
            //__instance.UsingStock = false;
            //__instance.StockAmount = -1; // not working?
            __instance.SetStock(-1, false);
            __instance.IsFree = true; // stupid beta wall still shows 5$

            return false;
        }

        [HarmonyPatch(typeof(LevelEditorObjectInfoViewModel), nameof(LevelEditorObjectInfoViewModel.ObjectsSelectedText), MethodType.Getter), HarmonyPrefix]
        public static bool RemoveMultiselectLimitText(LevelEditorObjectInfoViewModel __instance, out string __result)
        {
            __result = LevelEditor.LevelEditorMultiSelectionHandler.Selection().Count + __instance._localisedStrings.GetString("wle_objectsselected").Replace("<number>", "") + ((MainFeaturePatches.fDistance_threshold != 10.0f) ? " R: "+MainFeaturePatches.fDistance_threshold : "");
            return false;
        }
        #endregion
    }
}
