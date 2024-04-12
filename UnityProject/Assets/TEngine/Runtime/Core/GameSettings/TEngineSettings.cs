using UnityEngine;

[CreateAssetMenu(fileName = "TEngineGlobalSettings", menuName = "TEngine/TEngineSettings")]
public class TEngineSettings : ScriptableObject
{
    [Header("Framework")] [SerializeField] private FrameworkGlobalSettings m_FrameworkGlobalSettings;

    public FrameworkGlobalSettings FrameworkGlobalSettings => m_FrameworkGlobalSettings;

    [Header("HybridCLR")] [SerializeField] private HybridCLRCustomGlobalSettings m_HybridCLRCustomGlobalSettings;

    public HybridCLRCustomGlobalSettings BybridCLRCustomGlobalSettings => m_HybridCLRCustomGlobalSettings;

    public void Set(FrameworkGlobalSettings globalSettings,HybridCLRCustomGlobalSettings hybridClrCustomGlobalSettings)
    {
        m_FrameworkGlobalSettings = globalSettings;
        m_HybridCLRCustomGlobalSettings = hybridClrCustomGlobalSettings;
    }
}