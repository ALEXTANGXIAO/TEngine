using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;

namespace TEngine
{
    [InitializeOnLoad]
    public class SceneSwitchLeftButton
    {
        private static readonly string SceneMain = "main";

        static SceneSwitchLeftButton()
        {
            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
        }

        static readonly string ButtonStyleName = "Tab middle";
        static GUIStyle _buttonGuiStyle;

        static void OnToolbarGUI()
        {
            _buttonGuiStyle ??= new GUIStyle(ButtonStyleName)
            {
                padding = new RectOffset(2, 8, 2, 2),
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(
                    new GUIContent("Launcher", EditorGUIUtility.FindTexture("PlayButton"), $"Start Scene Launcher"),
                    _buttonGuiStyle))
            {
                SceneHelper.StartScene(SceneMain);
            }
        }
    }

    static class SceneHelper
    {
        static string _sceneToOpen;

        public static void StartScene(string sceneName)
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            _sceneToOpen = sceneName;
            EditorApplication.update += OnUpdate;
        }

        static void OnUpdate()
        {
            if (_sceneToOpen == null ||
                EditorApplication.isPlaying || EditorApplication.isPaused ||
                EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            EditorApplication.update -= OnUpdate;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                string[] guids = AssetDatabase.FindAssets("t:scene " + _sceneToOpen, null);
                if (guids.Length == 0)
                {
                    Debug.LogWarning("Couldn't find scene file");
                }
                else
                {
                    string scenePath = null;
                    // 优先打开完全匹配_sceneToOpen的场景
                    for (var i = 0; i < guids.Length; i++)
                    {
                        scenePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                        if (scenePath.EndsWith("/" + _sceneToOpen + ".unity"))
                        {
                            break;
                        }
                    }

                    // 如果没有完全匹配的场景，默认显示找到的第一个场景
                    if (string.IsNullOrEmpty(scenePath))
                    {
                        scenePath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    }

                    EditorSceneManager.OpenScene(scenePath);
                    EditorApplication.isPlaying = true;
                }
            }

            _sceneToOpen = null;
        }
    }
}