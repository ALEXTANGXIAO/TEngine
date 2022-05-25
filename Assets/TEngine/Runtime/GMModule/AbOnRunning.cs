#if ENABLE_GM || UNITY_EDITOR || _DEVELOPMENT_BUILD_
using System.Collections.Generic;
using UnityEngine;

namespace TEngine
{
    public class AbOnRunning : MonoBehaviour
    {
        private static bool hasInit = false;

        private static string filterName = "";
        private static int _count = 0;

        private static Vector2 _vec2ScollPos = new Vector2();

        private static Dictionary<string, AssetBundleData> _bundleDatasValue;

        static void DoInit()
        {
            if (hasInit)
            {
                return;
            }

            _bundleDatasValue = new Dictionary<string, AssetBundleData>();

            hasInit = true;
        }

        private static void ToogleScrollView1()
        {
            GUILayout.BeginVertical("box");

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("AB名:", GUILayout.MaxWidth(100));
            filterName = GUILayout.TextField(filterName);
            filterName = filterName.ToLower();
            GUILayout.EndHorizontal();


            GUIStyle curStyle = new GUIStyle("box");
            curStyle.alignment = TextAnchor.MiddleLeft;
            curStyle.fontSize = Screen.width / 40;
            GUILayout.Label($"引用数-被引用数-AbName-当前激活数 = {_count}", curStyle);

            _count = 0;
            _vec2ScollPos = GUILayout.BeginScrollView(_vec2ScollPos);
            foreach (var pair in _bundleDatasValue)
            {
                if (pair.Value.RefCount == 0 && pair.Value.DepCount == 0)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(filterName) && !pair.Value.Name.Contains(filterName))
                    continue;

                _count++;
                GUILayout.Label($"{pair.Value.RefCount}  {pair.Value.DepCount}  {pair.Value.Name}");
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        [CommonGMCommand("AB信息预览", "", mIsDrawer = true)]
        public static void GM_ABRuntime()
        {
            DoInit();
            ToogleScrollView1();
        }
    }
}
#endif