using UnityEngine;
using UnityEditor;

namespace TEngine.UI
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