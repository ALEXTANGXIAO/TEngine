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


#if UNITY_EDITOR
        [SerializeField] public bool EditorResourceMode = true;
#else
        private bool _editorResourceMode = false;
        public bool EditorResourceMode => _editorResourceMode;
#endif


        public override void Awake()
        {
            base.Awake();
        }
    }
}