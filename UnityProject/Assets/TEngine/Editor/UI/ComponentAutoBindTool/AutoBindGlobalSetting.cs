using Sirenix.OdinInspector;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 自动绑定全局设置
/// </summary>
public class AutoBindGlobalSetting : ScriptableObject
{
    [FolderPath]
    [LabelText("默认组件代码保存路径")]
    [SerializeField]
    private string m_CodePath;

    [LabelText("绑定代码命名空间")]
    [SerializeField]
    private string m_Namespace;

    [LabelText("子组件名称(不会往下继续遍历)")]
    [SerializeField]
    private string m_WidgetName = "m_item";
    
    public string CodePath => m_CodePath;

    public string Namespace => m_Namespace;
    
    public string WidgetName => m_WidgetName;

    [MenuItem("TEngine/CreateAutoBindGlobalSetting")]
    private static void CreateAutoBindGlobalSetting()
    {
        string[] paths = AssetDatabase.FindAssets("t:AutoBindGlobalSetting");
        if (paths.Length >= 1)
        {
            string path = AssetDatabase.GUIDToAssetPath(paths[0]);
            EditorUtility.DisplayDialog("警告", $"已存在AutoBindGlobalSetting，路径:{path}", "确认");
            return;
        }

        AutoBindGlobalSetting setting = CreateInstance<AutoBindGlobalSetting>();
        AssetDatabase.CreateAsset(setting, "Assets/TEngine/ResRaw/AutoBindGlobalSetting.asset");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}