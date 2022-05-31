using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace TEngine
{
    public static class Utils
    {

        #region Show
        public static void Show(this Graphic graphic, bool value)
        {
            if (graphic != null && graphic.transform != null)
            {
                graphic.transform.localScale = value ? Vector3.one : Vector3.zero;
            }
        }

        public static void Show(this GameObject go, bool value)
        {
            if (go != null && go.transform != null)
            {
                go.transform.localScale = value ? Vector3.one : Vector3.zero;
            }
        }

        public static void Show(this Transform transform, bool value)
        {
            if (transform != null)
            {
                transform.localScale = value ? Vector3.one : Vector3.zero;
            }
        }

        public static void Show(this Graphic graphic, bool value, ref bool cacheValue)
        {
            if (graphic != null && graphic.transform != null && value != cacheValue)
            {
                graphic.transform.localScale = value ? Vector3.one : Vector3.zero;
            }
        }

        public static void SetActive(this GameObject go, bool value, ref bool cacheValue)
        {
            if (go != null && value != cacheValue)
            {
                cacheValue = value;
                go.SetActive(value);
            }
        }


        #endregion

        #region IEnumerator
        public static IEnumerator Wait(float second, Action action = null)
        {
            if (second <= 0)
            {
                yield break;
            }

            yield return new WaitForSeconds(second);

            if (action != null)
            {
                action();
            }
        }


        #endregion

        #region Regex
        /// <summary>
        /// 直接用Regex.IsMatch(xxx,xxx)每次都会New对象
        /// </summary>
        private static Dictionary<string, Regex> _regexes = new Dictionary<string, Regex>();

        /// <summary>
        /// 字符串是否匹配(包含/不区分大小写)
        /// </summary>
        /// <param name="inputStr">inputStr</param>
        /// <param name="keyStr">keyStr</param>
        /// <param name="options">匹配模式(IgnoreCase/None)</param>
        /// <returns>bool inputStr isMatch keyStr ?</returns>
        public static bool IsMatch(string inputStr, string keyStr, RegexOptions options = RegexOptions.IgnoreCase)
        {
            if (inputStr == null || keyStr == null)
            {
                return false;
            }

            Regex regex;

            if (_regexes.TryGetValue(keyStr, out regex))
            {
                var isMatchSuccess = regex.IsMatch(inputStr);

                return isMatchSuccess;
            }
            else
            {
                regex = new Regex(keyStr, options);

                _regexes.Add(keyStr, regex);

                return regex.IsMatch(inputStr);
            }
        }


        #endregion

        #region SetSprite
        /// <summary>
        /// Image更改Sprite接口
        /// </summary>
        /// <param name="image">Image对象</param>
        /// <param name="path">Sprite路径，通过右键菜单Get Asset Path获取的路径</param>
        /// <param name="bAsync">是否异步加载</param>
        /// <remarks>置空时传入path为null</remarks>
        public static void SetSprite(this UnityEngine.UI.Image image, string path, bool bAsync = false)
        {
            LoadAsset<Sprite>(image, path, 0);
        }

        /// <summary>
        /// SpriteRenderer更改Sprite接口
        /// </summary>
        /// <param name="spriteRenderer">SpriteRenderer对象</param>
        /// <param name="path">Sprite路径，通过右键菜单Get Asset Path获取的路径</param>
        /// <param name="bAsync">是否异步加载</param>
        /// <remarks>置空时传入path为null</remarks>
        public static void SetSprite(this SpriteRenderer spriteRenderer, string path)
        {
            LoadAsset<Sprite>(spriteRenderer, path, 0);
        }

        static void LoadAsset<T>(Component component, string path, int index) where T : UnityEngine.Object
        {
            if (component == null)
            {
                TLogger.LogException("component is null");
                return;
            }

            if (string.IsNullOrEmpty(path))
            {
                if (component as Image != null)
                {
                    var image = (Image)component;
                    image.sprite = null;
                }
                else if(component as SpriteRenderer != null)
                {
                    var image = (SpriteRenderer)component;
                    image.sprite = null;
                }
            }
            else
            {
                int splitIndex = path.LastIndexOf('#');
                string resPath;
                bool bWithSubAssets;
                string subAssetName;
                if (splitIndex > 0)
                {
                    resPath = path.Substring(0, splitIndex);
                    subAssetName = path.Substring(splitIndex + 1);
                    bWithSubAssets = true;
                }
                else
                {
                    resPath = path;
                    if (typeof(T) == typeof(Sprite))
                    {
                        bWithSubAssets = true;
                        subAssetName = Path.GetFileNameWithoutExtension(path);
                    }
                    else
                    {
                        bWithSubAssets = false;
                    }
                }

                var asset = ResMgr.Instance.GetAsset(resPath, bWithSubAssets);

                if (asset == null)
                {
                    return;
                }

                if (component.GetType() == typeof(Image))
                {
                    var image = (Image)component;
                    image.sprite = asset.AssetObject as Sprite;
                }
                else if (component as SpriteRenderer != null)
                {
                    var spriteRenderer = (SpriteRenderer)component;
                    spriteRenderer.sprite = asset.AssetObject as Sprite;
                }
            }
        }
        #endregion

        #region Sort
        /// <summary>
        /// 使用稳定排序，使sort index相等时更新列表，任务列表项的顺序不变
        /// c# list自带的排序是不稳定排序
        /// 本排序方式为插入排序
        /// </summary>
        public static void InsertSort<T>(List<T> array, Comparison<T> comparison)
        {
            var count = array.Count;
            for (var i = 1; i < count; i++)
            {
                var temp = array[i];
                for (var j = i - 1; j >= 0; j--)
                {
                    if (comparison(array[j], temp) > 0)
                    {
                        array[j + 1] = array[j];
                        array[j] = temp;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// c# list自带的排序是不稳定排序
        /// 本排序方式为快速排序,从小到大
        /// </summary>
        public static void QuickSort<T>(this List<T> array, Comparison<T> comparison)
        {
            QuickSortIndex(array, 0, array.Count - 1, comparison);
        }

        private static void QuickSortIndex<T>(this List<T> array, int startIndex, int endIndex, Comparison<T> comparison)
        {
            if (startIndex >= endIndex)
            {
                return;
            }
            var pivotIndex = QuickSortQnce<T>(array, startIndex, endIndex, comparison);
            QuickSortIndex(array, startIndex, pivotIndex - 1, comparison);
            QuickSortIndex(array, pivotIndex + 1, endIndex, comparison);
        }

        private static int QuickSortQnce<T>(List<T> array, int startIndex, int endIndex, Comparison<T> comparison)
        {
            while (startIndex < endIndex)
            {
                var num = array[startIndex];
                if (comparison(num, array[startIndex + 1]) > 0)
                {
                    array[startIndex] = array[startIndex + 1];
                    array[startIndex + 1] = num;
                    startIndex++;
                }
                else
                {
                    var temp = array[endIndex];
                    array[endIndex] = array[startIndex + 1];
                    array[startIndex + 1] = temp;
                    endIndex--;
                }
            }

            return startIndex;
        }
        #endregion
    }
}
