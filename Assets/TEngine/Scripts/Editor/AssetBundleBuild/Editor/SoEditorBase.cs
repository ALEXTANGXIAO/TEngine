using UnityEngine;

namespace TEngineCore.Editor
{
    public class SoEditorBase : ScriptableObject
    {

        public virtual void AdditionalInit()
        {
        }

        public virtual void OnAnyThingChange(string args)
        {
        }
    }
}