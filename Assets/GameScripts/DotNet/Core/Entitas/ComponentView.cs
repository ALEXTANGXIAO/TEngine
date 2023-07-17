#if UNITY_EDITOR
using UnityEngine;

namespace TEngine
{
    public class ComponentView: MonoBehaviour
    {
        public Entity Component
        {
            get;
            set;
        }
    }
}
#endif