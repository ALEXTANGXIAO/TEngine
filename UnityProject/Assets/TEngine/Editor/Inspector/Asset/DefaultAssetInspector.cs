using UnityEngine;
using UnityEditor;
using System.IO;

[CanEditMultipleObjects, CustomEditor(typeof(DefaultAsset), false)]
public class DefaultAssetInspector : Editor
{
    private const int MaxColum = 10240;
    
    private GUIStyle _textStyle;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (_textStyle == null)
        {
            _textStyle = "ScriptText";
        }

        bool enabled = GUI.enabled;
        GUI.enabled = true;
        string assetPath = AssetDatabase.GetAssetPath(target);
        if (assetPath.EndsWith(".lua") || 
            assetPath.EndsWith(".properties") || 
            assetPath.EndsWith(".gradle")
            )
        {
            string luaFile = File.ReadAllText(assetPath);
            string text;
            if (targets.Length > 1)
            {
                text = Path.GetFileName(assetPath);
            }
            else
            {
                text = luaFile;
                if (text.Length > MaxColum)
                {
                    text = text.Substring(0, MaxColum) + "...\n\n<...etc...>";
                }
            }

            Rect rect = GUILayoutUtility.GetRect(new GUIContent(text), _textStyle);
            rect.x = 0f;
            rect.y -= 3f;
            rect.width = EditorGUIUtility.currentViewWidth + 1f;
            GUI.Box(rect, text, _textStyle);
        }

        GUI.enabled = enabled;
    }
}