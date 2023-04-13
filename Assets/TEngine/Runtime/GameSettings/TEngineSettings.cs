using UnityEngine;

[CreateAssetMenu(fileName = "TEngineGlobalSettings", menuName = "TEngine/TEngineSettings")]
public class TEngineSettings : ScriptableObject
{
    [Header("Framework")] [SerializeField] private FrameworkGlobalSettings m_FrameworkGlobalSettings;

    public FrameworkGlobalSettings FrameworkGlobalSettings
    {
        get { return m_FrameworkGlobalSettings; }
    }

    [Header("HybridCLR")] [SerializeField] private HybridCLRCustomGlobalSettings m_BybridCLRCustomGlobalSettings;

    public HybridCLRCustomGlobalSettings BybridCLRCustomGlobalSettings
    {
        get { return m_BybridCLRCustomGlobalSettings; }
    }
}
