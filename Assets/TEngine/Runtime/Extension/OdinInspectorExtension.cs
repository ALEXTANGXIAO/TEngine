using System;
using System.Diagnostics;
using UnityEngine;

#if !ODIN_INSPECTOR
namespace Sirenix.OdinInspector
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class ButtonAttribute : Attribute
    {
        public ButtonAttribute()
        {
        }

        public ButtonAttribute(string name)
        {
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public sealed class ReadOnlyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
    [Conditional("UNITY_EDITOR")]
    public class ShowInInspectorAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public sealed class HideIfAttribute : Attribute
    {
        public string MemberName;
        public object Value;
        public bool Animate;

        public HideIfAttribute(string memberName, bool animate = true)
        {
            this.MemberName = memberName;
            this.Animate = animate;
        }
    }

    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public sealed class OnValueChangedAttribute : Attribute
    {
        public string MethodName;

        public bool IncludeChildren;

        public OnValueChangedAttribute(string methodName, bool includeChildren = false)
        {
            this.MethodName = methodName;
            this.IncludeChildren = includeChildren;
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    [Conditional("UNITY_EDITOR")]
    public class TableListAttribute : Attribute
    {
        public int DefaultMinColumnWidth = 40;
        public bool DrawScrollView = true;

        public int MinScrollViewHeight = 350;

        public int CellPadding = 2;

        public int NumberOfItemsPerPage;

        public bool IsReadOnly;

        public bool ShowIndexLabels;

        public int MaxScrollViewHeight;

        public bool AlwaysExpanded;

        public bool HideToolbar;

        [SerializeField] [HideInInspector] private bool showPagingHasValue;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DontApplyToListElementsAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [DontApplyToListElements]
    [Conditional("UNITY_EDITOR")]
    public class PropertySpaceAttribute : Attribute
    {
        public float SpaceBefore;
        public float SpaceAfter;

        public PropertySpaceAttribute()
        {
            this.SpaceBefore = 8f;
            this.SpaceAfter = 0.0f;
        }

        public PropertySpaceAttribute(float spaceBefore)
        {
            this.SpaceBefore = spaceBefore;
            this.SpaceAfter = 0.0f;
        }

        public PropertySpaceAttribute(float spaceBefore, float spaceAfter)
        {
            this.SpaceBefore = spaceBefore;
            this.SpaceAfter = spaceAfter;
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [Conditional("UNITY_EDITOR")]
    public class LabelTextAttribute : Attribute
    {
        public string Text;

        public LabelTextAttribute(string text)
        {
            this.Text = text;
        }
    }
}
#endif