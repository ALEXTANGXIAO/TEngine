using System.Collections;
using System.Collections.Generic;
using TEngine.Runtime;
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
        [SerializeField]
        public ResourceMode ResourceMode = ResourceMode.Package;


#if UNITY_EDITOR
        [SerializeField]
        public bool EditorResourceMode = true;
#else
        public bool EditorResourceMode = false;
#endif


        public override void Awake()
        {
            base.Awake();
        }
    }

}