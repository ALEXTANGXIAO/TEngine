using UnityEngine;

namespace GameMain
{
    public class UIBase : MonoBehaviour
    {
        protected object Param;
        public virtual void OnEnter(object param)
        {
            Param = param;
        }
    }
}