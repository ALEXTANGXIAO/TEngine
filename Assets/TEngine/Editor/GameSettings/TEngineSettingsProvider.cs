using System.IO;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;

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

    public override void OnGUI(string searchContext)
    {
        base.OnGUI(searchContext);
        using var changeCheckScope = new EditorGUI.ChangeCheckScope();
        EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("m_FrameworkGlobalSettings"));
        EditorGUILayout.PropertyField(m_CustomSettings.FindProperty("m_BybridCLRCustomGlobalSettings"));
        EditorGUILayout.Space(20);
        if (!changeCheckScope.changed) return;
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
            UnityEngine.Debug.LogError($"Open GameFramework Settings error,Please Create Game Framework/GameFrameworkSettings.assets File in Path GameMain/Resources/Settings");
        }

        return null;
    }
}