using UnityEngine;


namespace TEngine.Runtime
{
    /// <summary>
    /// 资源组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("TEngine/Resource")]
    public class ResourceComponent : UnitySingleton<ResourceComponent>
    {
        [SerializeField] public ResourceMode ResourceMode = ResourceMode.Package;

        public override void Awake()
        {
            base.Awake();
            
            ResourceHelperBase resourceHelper = null;
            
            resourceHelper = Helper.CreateHelper(m_ResourceHelperTypeName, m_CustomResourceHelper);
            if (resourceHelper == null)
            {
                Log.Error("Can not create resource helper.");
                return;
            }

            if (resourceHelper != null)
            {
                TResources.SetResourceHelper(resourceHelper);
            }
        }
        
        [SerializeField]
        private string m_ResourceHelperTypeName = "TEngine.Runtime.DefaultResourceHelper";

        [SerializeField]
        private ResourceHelperBase m_CustomResourceHelper = null;
    }
}