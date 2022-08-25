using UnityEditor;

namespace TEngineCore.Editor
{
    internal interface IBuilder
    {
        void SetBuilderConfig(BuilderUtility.PlatformType target, string configName, string configPath = "");
        void SetBuilderConfig(BuilderEditor tmpBuilder);

        void SwitchPlatform(BuilderUtility.PlatformType target);
        bool BuildAssetBundle();

        void BuildPackage(BuildOptions option = BuildOptions.None);
    }
}