using System.Net;
using UnityEngine;
using Il2CppInterop.Common.Attributes;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppInterop.Runtime.Runtime;
using Il2CppSystem;
using Il2CppSystem.Collections;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.Text;
using static RootMotion.FinalIK.RagdollUtility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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

        public static System.Collections.Generic.List<GameObject> FindAllChildren(this Transform Parent, string Name)
        {
            System.Collections.Generic.List<GameObject> listOfChildren = new System.Collections.Generic.List<GameObject>();
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
    }
}
