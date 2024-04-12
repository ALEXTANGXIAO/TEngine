using System.IO;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine;

public class TEngineSettingsProvider : SettingsProvider
{
    const string k_SettingsPath = "Assets/TEngine/ResRaw/Resources/TEngineGlobalSettings.asset";
    private const string headerName = "TEngine/TEngineSettings";
    private SerializedObject m_CustomSettings;

    internal static SerializedObject GetSerializedSettings()
    {
        return new SerializedObject(SettingsUtils.GlobalSettings);
    }

    public static bool IsSettingsAvailable()
    {
        return File.Exists(k_SettingsPath);
    }

    public override void OnActivate(string searchContext, VisualElement rootElement)
    {
        base.OnActivate(searchContext, rootElement);
        m_CustomSettings = GetSerializedSettings();
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
        SaveAssetData(k_SettingsPath);
    }
    
    void SaveAssetData(string path)
    {
        TEngineSettings old = AssetDatabase.LoadAssetAtPath<TEngineSettings>(k_SettingsPath);
        TEngineSettings data = ScriptableObject.CreateInstance<TEngineSettings>();
        data.Set(old.FrameworkGlobalSettings, old.BybridCLRCustomGlobalSettings);
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(data, path);              
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


    public override void OnGUI(string searchContext)
    {
        base.OnGUI(searchContext);
        using var changeCheckScope = new EditorGUI.ChangeCheckScope();
        EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("m_FrameworkGlobalSettings"));

        if (GUILayout.Button("Refresh HotUpdateAssemblies"))
        {
            SyncAssemblyContent.RefreshAssembly();
            m_CustomSettings.ApplyModifiedPropertiesWithoutUndo();
            m_CustomSettings = null;
            m_CustomSettings = GetSerializedSettings();
        }

        EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("m_HybridCLRCustomGlobalSettings"));
        EditorGUILayout.Space(20);
        if (!changeCheckScope.changed)
        {
            return;
        }
        m_CustomSettings.ApplyModifiedPropertiesWithoutUndo();
    }

    public TEngineSettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
    {
    }

    [SettingsProvider]
    private static SettingsProvider CreateSettingProvider()
    {
        if (IsSettingsAvailable())
        {
            var provider = new TEngineSettingsProvider(headerName, SettingsScope.Project);
            provider.keywords = GetSearchKeywordsFromGUIContentProperties<TEngineSettings>();
            return provider;
        }
        else
        {
            Debug.LogError($"Open TEngine Settings error,Please Create TEngine TEngineGlobalSettings.assets File in Path TEngine/ResRaw/Resources/");
        }

        return null;
    }
}