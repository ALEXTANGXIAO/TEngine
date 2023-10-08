using UnityEditor;

public static class SettingsMenu
{
    [MenuItem("TEngine/Settings/TEngineSettings", priority = -1)]
    public static void OpenSettings() => SettingsService.OpenProjectSettings("TEngine/TEngineSettings");
}