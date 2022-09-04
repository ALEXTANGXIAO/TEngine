using System.Collections.Generic;
using UnityEngine;

namespace TEngine.Runtime
{
    public static partial class Utility
    {
        /// <summary>
        /// GameObject相关的实用函数。
        /// </summary>
        public static class GameObjectUtils
        {
            #region Static Method

            /// <summary>
            /// 克隆实例
            /// </summary>
            /// <typeparam name="T">实例类型</typeparam>
            /// <param name="original">初始对象</param>
            /// <returns>克隆的新对象</returns>
            public static T Clone<T>(T original) where T : Object
            {
                return Object.Instantiate(original);
            }

            /// <summary>
            /// 克隆实例
            /// </summary>
            /// <typeparam name="T">实例类型</typeparam>
            /// <param name="original">初始对象</param>
            /// <param name="position">新对象的位置</param>
            /// <param name="rotation">新对象的旋转</param>
            /// <returns>克隆的新对象</returns>
            public static T Clone<T>(T original, Vector3 position, Quaternion rotation) where T : Object
            {
                return Object.Instantiate(original, position, rotation);
            }

            /// <summary>
            /// 克隆实例
            /// </summary>
            /// <typeparam name="T">实例类型</typeparam>
            /// <param name="original">初始对象</param>
            /// <param name="position">新对象的位置</param>
            /// <param name="rotation">新对象的旋转</param>
            /// <param name="parent">新对象的父物体</param>
            /// <returns>克隆的新对象</returns>
            public static T Clone<T>(T original, Vector3 position, Quaternion rotation, Transform parent)
                where T : Object
            {
                return Object.Instantiate(original, position, rotation, parent);
            }

            /// <summary>
            /// 克隆实例
            /// </summary>
            /// <typeparam name="T">实例类型</typeparam>
            /// <param name="original">初始对象</param>
            /// <param name="parent">新对象的父物体</param>
            /// <returns>克隆的新对象</returns>
            public static T Clone<T>(T original, Transform parent) where T : Object
            {
                return Object.Instantiate(original, parent);
            }

            /// <summary>
            /// 克隆实例
            /// </summary>
            /// <typeparam name="T">实例类型</typeparam>
            /// <param name="original">初始对象</param>
            /// <param name="parent">新对象的父物体</param>
            /// <param name="worldPositionStays">是否保持世界位置不变</param>
            /// <returns>克隆的新对象</returns>
            public static T Clone<T>(T original, Transform parent, bool worldPositionStays) where T : Object
            {
                return Object.Instantiate(original, parent, worldPositionStays);
            }

            /// <summary>
            /// 克隆 GameObject 实例
            /// </summary>
            /// <param name="original">初始对象</param>
            /// <param name="isUI">是否是UI对象</param>
            /// <returns>克隆的新对象</returns>
            public static GameObject CloneGameObject(GameObject original, bool isUI = false)
            {
                GameObject obj = Object.Instantiate(original);
                obj.transform.SetParent(original.transform.parent);
                if (isUI)
                {
                    RectTransform rect = obj.rectTransform();
                    RectTransform originalRect = original.rectTransform();
                    rect.anchoredPosition3D = originalRect.anchoredPosition3D;
                    rect.sizeDelta = originalRect.sizeDelta;
                    rect.offsetMin = originalRect.offsetMin;
                    rect.offsetMax = originalRect.offsetMax;
                    rect.anchorMin = originalRect.anchorMin;
                    rect.anchorMax = originalRect.anchorMax;
                    rect.pivot = originalRect.pivot;
                }
                else
                {
                    obj.transform.localPosition = original.transform.localPosition;
                }

                obj.transform.localRotation = original.transform.localRotation;
                obj.transform.localScale = original.transform.localScale;
                obj.SetActive(true);
                return obj;
            }

            /// <summary>
            /// 杀死实例
            /// </summary>
            /// <param name="obj">实例对象</param>
            public static void Kill(Object obj)
            {
                Object.Destroy(obj);
            }

            /// <summary>
            /// 立即杀死实例
            /// </summary>
            /// <param name="obj">实例对象</param>
            public static void KillImmediate(Object obj)
            {
                Object.DestroyImmediate(obj);
            }

            /// <summary>
            /// 杀死一群实例
            /// </summary>
            /// <typeparam name="T">实例类型</typeparam>
            /// <param name="objs">实例集合</param>
            public static void Kills<T>(List<T> objs) where T : Object
            {
                for (int i = 0; i < objs.Count; i++)
                {
                    Object.Destroy(objs[i]);
                }

                objs.Clear();
            }

            /// <summary>
            /// 杀死一群实例
            /// </summary>
            /// <typeparam name="T">实例类型</typeparam>
            /// <param name="objs">实例数组</param>
            public static void Kills<T>(T[] objs) where T : Object
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    Object.Destroy(objs[i]);
                }
            }

            #endregion
        }
    }

    public static class GameObjectExt
    {
        /// <summary>
        /// 获取RectTransform组件
        /// </summary>
        public static RectTransform rectTransform(this GameObject obj)
        {
            return obj.GetComponent<RectTransform>();
        }

        /// <summary>
        /// 获取RectTransform组件
        /// </summary>
        public static RectTransform rectTransform(this MonoBehaviour mono)
        {
            return mono.GetComponent<RectTransform>();
        }
    }
}