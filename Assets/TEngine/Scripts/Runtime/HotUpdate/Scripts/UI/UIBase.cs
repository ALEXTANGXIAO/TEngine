using UnityEngine;

namespace TEngine.Runtime.HotUpdate
{
    public class UIBase : MonoBehaviour
    {
        protected object Param;
        virtual public void OnEnter(object param)
        {
            Param = param;
        }
    }
}