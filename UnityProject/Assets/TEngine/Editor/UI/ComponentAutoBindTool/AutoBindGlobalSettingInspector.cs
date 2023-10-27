using Sirenix.OdinInspector.Editor;
using UnityEditor;

[CustomEditor(typeof(AutoBindGlobalSetting))]
public class AutoBindGlobalSettingInspector : OdinEditor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.ApplyModifiedProperties();
    }
}