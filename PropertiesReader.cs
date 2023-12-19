using System.Collections.Generic;
using UnityEngine;
using System.IO;
using ScriptableObjects;
using System.Xml.Linq;
using System.Xml.XPath;

namespace FraggleExpansion
{
    public class PropertiesReader
    {
        public static PropertiesReader Instance;
        public PropertiesReader() => InitializeData();

        public void InitializeData()
        {
            Instance = this;
            List<string> CleanContent = new List<string>();
            string FilePath =
            Path.Combine(BepInEx.Paths.GameRootPath + "\\BepInEx\\plugins\\CreativeExpansionPack\\ExpansionData.txt");
            if (!File.Exists(FilePath)) { /*Application.Quit();*/ return; } // no need to quit bruh, we've got default values
            string[] AllLinesInFile = File.ReadAllLines(FilePath);

            string[] CommentOptions =
            {
                "//",
                "///",
                "#"
            };

            // Get only relevant data
            foreach (string Line in AllLinesInFile)
                foreach (string Comment in CommentOptions)
                    if (Line.StartsWith(Comment) || Line == "") continue;
                    else CleanContent.Add(Line);

            // Configure data
            foreach (string Data in CleanContent)
            {
                string[] SplitData = Data.Split(':');
                bool ResultAsBoolean = true;
                //float ResultAsFloat = 0.025f;

                switch (SplitData[0])
                {
                    // Booleans

                    /*case "removecostandstock":
                        ReadBool(SplitData[1], ref ResultAsBoolean, FraggleExpansionData.RemoveCostAndStock);
                        FraggleExpansionData.RemoveCostAndStock = ResultAsBoolean;
                        break;

                    case "removerotation":
                        ReadBool(SplitData[1], ref ResultAsBoolean, FraggleExpansionData.RemoveRotation);
                        FraggleExpansionData.RemoveRotation = ResultAsBoolean;
                        break;*/

                    case "BypassBounds":
                        ReadBool(SplitData[1], ref ResultAsBoolean, FraggleExpansionData.BypassBounds);
                        FraggleExpansionData.BypassBounds = ResultAsBoolean;
                        break;

                    /*case "betawalls":
                        ReadBool(SplitData[1], ref ResultAsBoolean, FraggleExpansionData.BetaWalls);
                        FraggleExpansionData.BetaWalls = ResultAsBoolean;
                        break;*/

                    /*case "displaylevel":
                        ReadBool(SplitData[1], ref ResultAsBoolean, FraggleExpansionData.DisplayLevel);
                        FraggleExpansionData.DisplayLevel = ResultAsBoolean;
                        break;*/

                    case "CustomBuilderSkin":
                        ReadBool(SplitData[1], ref ResultAsBoolean, FraggleExpansionData.UseMainSkinInExploreState);
                        FraggleExpansionData.UseMainSkinInExploreState = ResultAsBoolean;
                        break;

                    case "LastPosition":
                        ReadBool(SplitData[1], ref ResultAsBoolean, FraggleExpansionData.LastPostion);
                        FraggleExpansionData.LastPostion = ResultAsBoolean;
                        break;

                    /*case "letobjectsclip":
                        ReadBool(SplitData[1], ref ResultAsBoolean, FraggleExpansionData.CanClipObjects);
                        FraggleExpansionData.CanClipObjects = ResultAsBoolean;
                        break;*/

                    /*case "customtestmusic":
                        ReadBool(SplitData[1], ref ResultAsBoolean, FraggleExpansionData.CustomTestMusic);
                        FraggleExpansionData.CustomTestMusic = ResultAsBoolean;
                        break;*/

                    /*case "insanepaintersize":
                        ReadBool(SplitData[1], ref ResultAsBoolean, FraggleExpansionData.InsanePainterSize);
                        FraggleExpansionData.InsanePainterSize = ResultAsBoolean;
                        break;

                    case "levelmusic":
                        FraggleExpansionData.LevelMusic = SplitData[1];
                        break;*/

                    /*case "ghostblocks":
                        ReadBool(SplitData[1], ref ResultAsBoolean, FraggleExpansionData.GhostBlocks);
                        FraggleExpansionData.GhostBlocks = ResultAsBoolean;
                        break;*/

                    /*case "snapSeparatorSize":
                        ReadFloat(SplitData[1], ref ResultAsFloat, FraggleExpansionData.snapSeparatorSize);
                        FraggleExpansionData.snapSeparatorSize = ResultAsFloat;
                        Main.Instance.Log.LogMessage("snapSeparatorSize: " + FraggleExpansionData.snapSeparatorSize);
                        break;*/

                    case "AddCustomObjects":
                        ReadBool(SplitData[1], ref ResultAsBoolean, FraggleExpansionData.AddUnusedObjects);
                        FraggleExpansionData.AddUnusedObjects = ResultAsBoolean;
                        ReadObjectsToAddData();
                        break;

                    case "AddAllObjects":
                        ReadBool(SplitData[1], ref ResultAsBoolean, FraggleExpansionData.AddAllObjects);
                        FraggleExpansionData.AddAllObjects = ResultAsBoolean;
                        break;

                    case "RemoveDefaultScalingFeature":
                        ReadBool(SplitData[1], ref ResultAsBoolean, FraggleExpansionData.RemoveDefaultScalingFeature);
                        FraggleExpansionData.RemoveDefaultScalingFeature = ResultAsBoolean;
                        break;

                        /*case "musiceventplaymode":
                            FraggleExpansionData.MusicEventPlayMode = SplitData[1];
                            break;

                        case "musicbankplaymode":
                            FraggleExpansionData.MusicBankPlayMode = SplitData[1];
                            break;*/



                        // CEP generated and pretty bad, it works tho...
                        // Do not touch (I know it's bad but still) // Imma touch this!

                        /*case "letfirsttimepopuphappen":
                            ReadBool(SplitData[1], ref ResultAsBoolean, FraggleExpansionData.LetFirstTimePopUpHappen);
                            FraggleExpansionData.LetFirstTimePopUpHappen = ResultAsBoolean;
                            break;*/
                }

            }
        }

        public void ReadObjectsToAddData()
        {
            string FilePath =
            Path.Combine(BepInEx.Paths.GameRootPath + "\\BepInEx\\plugins\\CreativeExpansionPack\\Object_Data.txt");
            if (!File.Exists(FilePath)) { Application.Quit(); return; }
            FraggleExpansionData.AddObjectData = File.ReadAllLines(FilePath);
        }

        /*public static void WriteFirstTimePopUpGone(string Prop)
        {
            if (!FraggleExpansionData.LetFirstTimePopUpHappen) return;
            string FilePath = Path.Combine(BepInEx.Paths.GameRootPath + "\\BepInEx\\plugins\\CreativeExpansionPack\\ExpansionData.txt");
            
            // Totally didn't ChatGPT out of laziness
            using (StreamWriter Writer = File.AppendText(FilePath))
            {
                Writer.WriteLine(Prop + ":false");
            }
        }*/

        public void ReadBool(string Data, ref bool Value, bool DefaultValue)
        {
            if (Data.ToLower() == "true") Value = true;
            else if (Data.ToLower() == "false") Value = false;
            else Value = DefaultValue;
        }


        /*public void ReadFloat(string Data, ref float Value, float BaseResult)
        {
            Value = BaseResult;
            try { Value = float.Parse(Data); } catch { }
        }*/

    }

    public class myXml
    {
        public static myXml Instance;
        public XDocument Data = XDocument.Load(FilePath);
        public myXml() => Init();
        static string FilePath = Path.Combine(BepInEx.Paths.GameRootPath + "\\BepInEx\\plugins\\CreativeExpansionPack\\InGameRadialButtonsStates.xml");
        public void Init()
        {
            Instance = this;
            FraggleExpansionData.GhostBlocks = Data.XPathSelectElement("/States/GhostBLocks").Value == "True";
        }

        public void Save()
        {
            Data.Save(FilePath);
        }
    }
    public struct FraggleExpansionData
    {
        // True booleans
        public static bool AddUnusedObjects = false, AddAllObjects = true, GhostBlocks = true, LastPostion = true, BypassBounds = true, UseMainSkinInExploreState = true, RemoveDefaultScalingFeature = true;
        //public static bool LetFirstTimePopUpHappen = true;
        //public static string MusicBankPlayMode = "BNK_Music_Long_Wall";
        //public static string MusicEventPlayMode = "MUS_InGame_Long_Wall";
        //public static float snapSeparatorSize = 0.025F;
        //public static string LevelMusic;
        public static string[] AddObjectData;
        public static Il2CppSystem.Collections.Generic.List<PlaceableVariant_Wall> vWalls;
        public static Il2CppSystem.Collections.Generic.List<PlaceableVariant_Wall> dWalls;
        public static Il2CppSystem.Collections.Generic.List<PlaceableVariant_Wall> bWalls;
        public static PlaceableVariant_Wall vWallPillars;
        public static PlaceableVariant_Wall dWallPillars;
        public static PlaceableVariant_Wall bWallPillars;
    }
}
