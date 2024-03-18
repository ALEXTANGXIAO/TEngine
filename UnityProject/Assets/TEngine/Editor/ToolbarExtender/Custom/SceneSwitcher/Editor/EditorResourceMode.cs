using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

namespace TEngine.Editor
{
    [InitializeOnLoad]
    public class EditorResourceMode
    {
        static class ToolbarStyles
        {
            public static readonly GUIStyle ToolBarButtonGuiStyle;

            static ToolbarStyles()
            {
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
            _playModeIndex = EditorPrefs.GetInt("EditorPlayMode", 0);
        }

        private const string ButtonStyleName = "Tab middle";
        static GUIStyle _buttonGuiStyle;

        private static readonly string[] _resourceModeNames =
        {
            "EditorMode (编辑器下的模拟模式)",
            "OfflinePlayMode (单机模式)",
            "HostPlayMode (联机运行模式)",
            "WebPlayMode (WebGL运行模式)"
        };

        private static int _playModeIndex = 0;
        public static int PlayModeIndex => _playModeIndex;

        static void OnToolbarGUI()
        {
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                GUILayout.Space(10);

                GUILayout.FlexibleSpace();

                // 资源模式
                int selectedIndex = EditorGUILayout.Popup("", _playModeIndex, _resourceModeNames, ToolbarStyles.ToolBarButtonGuiStyle);
                // ReSharper disable once RedundantCheckBeforeAssignment
                if (selectedIndex != _playModeIndex)
                {
                    Debug.Log($"更改编辑器资源运行模式 : {_resourceModeNames[selectedIndex]}");
                    _playModeIndex = selectedIndex;
                    EditorPrefs.SetInt("EditorPlayMode", selectedIndex);
                }

                GUILayout.FlexibleSpace();

                GUILayout.Space(400);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
}