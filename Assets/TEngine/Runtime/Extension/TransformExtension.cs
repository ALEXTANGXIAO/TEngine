using System.Collections.Generic;
using UnityEngine;

namespace TEngine
{
    public static partial class TransformExtension
    {
        public static float YDeg(this Transform tran)
        {
            return tran.eulerAngles.y;
        }

        public static void RemoveAllChildren(this Transform tran)
        {
            var count = tran.childCount;
            for (int i = 0; i < count; i++)
            {
                GameObject.DestroyImmediate(tran.GetChild(0).gameObject);
            }
        }

        public static List<Transform> GetAllChildren(this Transform tran)
        {
            var count = tran.childCount;
            List<Transform> allTrans = new List<Transform>();
            for (int i = 0; i < count; i++)
            {
                allTrans.Add(tran.GetChild(i));
            }

            return allTrans;
        }
    }
}