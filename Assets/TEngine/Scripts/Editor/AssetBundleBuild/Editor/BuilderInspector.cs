using UnityEditor;

namespace TEngineCore.Editor
{
    [CustomEditor(typeof(BuilderEditor))]
    public class BuilderInspector : InspectorBase
    {
        private BuilderEditor _target;

        protected override void OnEnable()
        {
            base.OnEnable();
            _target = target as BuilderEditor;
            if (_target != null)
                _target.OnInit();
        }
    }
}