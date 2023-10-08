using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace TEngine
{
    [InitializeOnLoad]
    public class EditorResourceMode
    {
        static class ToolbarStyles
        {
            public static readonly GUIStyle ToolBarExtenderBtnStyle;

            public static readonly GUIStyle ToolBarTextStyle;

            public static readonly GUIStyle ToolBarButtonGuiStyle;

            static ToolbarStyles()
            {
                ToolBarExtenderBtnStyle = new GUIStyle("Command")
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleCenter,
                    imagePosition = ImagePosition.ImageAbove,
                    fontStyle = FontStyle.Normal,
                    fixedWidth = 60
                };

                ToolBarTextStyle = new GUIStyle(ButtonStyleName)
                {
                    padding = new RectOffset(2, 8, 2, 2),
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };

                ToolBarButtonGuiStyle = new GUIStyle(ButtonStyleName)
                {
                    padding = new RectOffset(2, 8, 2, 2),
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold
                };
            }
        }

        static EditorResourceMode()
        {
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUI);
            _resourceModeIndex = EditorPrefs.GetInt("EditorResourceMode", 0);
        }

        private const string ButtonStyleName = "Tab middle";
        static GUIStyle _buttonGuiStyle;

        private static readonly string[] _resourceModeNames =
            { "EditorMode (编辑器下的模拟模式)",
                "OfflinePlayMode (单机模式)", 
                "HostPlayMode (联机运行模式)", 
                "WebPlayMode (WebGL运行模式)" 
            };

        private static int _resourceModeIndex = 0;
        public static int ResourceModeIndex => _resourceModeIndex;

        static void OnToolbarGUI()
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                // GUILayout.Label("资源加载模式:",ToolbarStyles.ToolBarTextStyle);

                GUILayout.Space(10);

                GUILayout.FlexibleSpace();

                // 资源模式
                int selectedIndex = EditorGUILayout.Popup("", _resourceModeIndex, _resourceModeNames, ToolbarStyles.ToolBarButtonGuiStyle);
                // ReSharper disable once RedundantCheckBeforeAssignment
                if (selectedIndex != _resourceModeIndex)
                {
                    Debug.Log($"更改编辑器资源运行模式 : {_resourceModeNames[selectedIndex]}");
                    _resourceModeIndex = selectedIndex;
                    EditorPrefs.SetInt("EditorResourceMode", selectedIndex);
                }

                GUILayout.FlexibleSpace();

                GUILayout.Space(400);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}