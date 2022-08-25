using UnityEngine;
using UnityEngine.Rendering;

namespace TEngine.Runtime
{
    public sealed partial class DebuggerComponent
    {
        private sealed class EnvironmentInformationWindow : ScrollableDebuggerWindowBase
        {
            private bool m_AbMode = false;
            public override void Initialize(params object[] args)
            {
#if ASSETBUNDLE_ENABLE
                m_AbMode = true;
#endif
            }

            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Environment Information</b>");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Product Name", Application.productName);
                    DrawItem("Company Name", Application.companyName);
                    DrawItem("Game Identifier", Application.identifier);
                    DrawItem("Game Framework Version", Version.GameFrameworkVersion);
                    DrawItem("Game Version", string.Format("{0} ({1})", Version.GameVersion, Version.InternalGameVersion.ToString()));
                    DrawItem("Resource Version",
                        m_AbMode ? 
                            "Unavailable in editor resource mode" : 
                            (string.IsNullOrEmpty(Version.ApplicableGameVersion) ? 
                                "Unknown" : 
                                string.Format("{0} ({1})",
                                    Version.ApplicableGameVersion,
                                    Version.InternalResourceVersion.ToString())));
                    DrawItem("Application Version", Application.version);
                    DrawItem("Unity Version", Application.unityVersion);
                    DrawItem("Platform", Application.platform.ToString());
                    DrawItem("System Language", Application.systemLanguage.ToString());
                    DrawItem("Cloud Project Id", Application.cloudProjectId);
                    DrawItem("Build Guid", Application.buildGUID);
                    DrawItem("Target Frame Rate", Application.targetFrameRate.ToString());
                    DrawItem("Internet Reachability", Application.internetReachability.ToString());
                    DrawItem("Background Loading Priority", Application.backgroundLoadingPriority.ToString());
                    DrawItem("Is Playing", Application.isPlaying.ToString());
                    DrawItem("Splash Screen Is Finished", SplashScreen.isFinished.ToString());
                    DrawItem("Run In Background", Application.runInBackground.ToString());
                    DrawItem("Install Name", Application.installerName);
                    DrawItem("Install Mode", Application.installMode.ToString());
                    DrawItem("Sandbox Type", Application.sandboxType.ToString());
                    DrawItem("Is Mobile Platform", Application.isMobilePlatform.ToString());
                    DrawItem("Is Console Platform", Application.isConsolePlatform.ToString());
                    DrawItem("Is Editor", Application.isEditor.ToString());
                    DrawItem("Is Debug Build", Debug.isDebugBuild.ToString());
                    DrawItem("Is Focused", Application.isFocused.ToString());
                    DrawItem("Is Batch Mode", Application.isBatchMode.ToString());
                }
                GUILayout.EndVertical();
            }
        }
    }
}
