using FG.Common.CMS;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem;
using Il2CppSystem.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using static RootMotion.FinalIK.RagdollUtility;

namespace FraggleExpansion
{
    /*public struct DataRisingSlime
    {
        public static float SlimeHeightPercentage = 100;
        public static float SlimeSpeedPercentage = 100;
    }*/

    public struct MiscData
    {
        public static GameObject CurrentPositionDisplay = null;
    }

    internal static class Tools
    {
        static readonly string[] SizeSuffixes = ["bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

        public static string SizeSuffix(int value, int decimalPlaces = 2, int decimalPlacesAfter = 2)
        {
            if (value == 0) { return "0"; }
            if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }

            int i = 0;
            double dValue = value;
            while (Math.Round(dValue, decimalPlaces) >= 1000)
            {
                dValue /= 1024;
                i++;
            }
            if (i < decimalPlacesAfter) decimalPlaces = 0;
            return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
        }
        // recursive
        /*public static GameObject FindChild(this GameObject Parent, string Name)
        {
            foreach (Transform g in Parent.GetComponentsInChildren<Transform>(true))
                if (g.name == Name) return g.gameObject;
            return null;
        }
        public static GameObject FindChild(this Transform Parent, string Name)
        {
            foreach (Transform g in Parent.GetComponentsInChildren<Transform>(true))
                if (g.name == Name) return g.gameObject;
            return null;
        }*/

        public static List<GameObject> FindAllChildren(this Transform Parent, string Name)
        {
            var listOfChildren = new List<GameObject>();
            foreach (Transform g in Parent.GetComponentsInChildren<Transform>(true))
                if (g.name == Name) listOfChildren.Add(g.gameObject);
            return listOfChildren;
        }

        /*public static List<GameObject> FindAllChildren(this GameObject Parent, string Name)
        {
            List<GameObject> listOfChildren = new List<GameObject>();
            foreach (Transform g in Parent.GetComponentsInChildren<Transform>(true))
                if (g.name == Name) listOfChildren.Add(g.gameObject);
            return listOfChildren;
        }
        //
        public static GameObject FindParentRecursive(this GameObject Child, string Name)
        {
            foreach (Transform Transform in Child.GetComponentsInParent<Transform>(true))
                if (Transform.name == Name) return Transform.gameObject;

            return null;
        }*/

        /*public static Sprite MakeOutAnIcon(string Link, int Width, int Height)
        {
            var WebC = new WebClient();
            byte[] ImageAsByte = WebC.DownloadData(Link);
            Texture2D Texture = new Texture2D(Width, Height, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
            if (ImageConversion.LoadImage(Texture, ImageAsByte))
            {
                Texture.filterMode = FilterMode.Point;
                return Sprite.Create(Texture, new Rect(0.0f, 0.0f, Texture.width, Texture.height), new Vector2(0.5f, 0.5f));
            }
            return null;
        }*/

        public static string LocalizeString(string key, string msg)
        {
            if (!CMSLoader.HasInstance) return msg;

            var locStrings = CMSLoader.Instance._localisedStrings;
            if (locStrings == null || locStrings._localisedStrings == null) return msg;

            if (locStrings._localisedStrings.TryAdd(key, msg))
                return key;

            return msg;
        }
    }
}
