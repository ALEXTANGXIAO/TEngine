using System;
using UnityEngine;

namespace TEngineCore.Editor
{
    internal class BuilderEditorAttribute : Attribute
    {
        public EditorContent Content;

        public BuilderEditorAttribute(string title)
        {
            Content = new EditorContent();
            Content.Title = title;
        }

        public BuilderEditorAttribute(string title, ContentType component, string extra)
        {
            Content = new EditorContent();
            Content.Title = title;
            Content.Component = component;
            Content.Extra = extra;
        }


        public BuilderEditorAttribute(string title, ContentType component)
        {
            Content = new EditorContent();
            Content.Title = title;
            Content.Component = component;
            Content.Extra = "";
        }

    }

    internal enum ContentType
    {
        Space,
        Button,
        Label,
        SelectableLabel,
        Enum,
        Toggle,
        TextField,
        Obj,
        ScrollLabel,
    }

    internal class EditorContent
    {
        public const string HorizontalStart = "HorizontalStart";
        public const string HorizontalEnd = "HorizontalEnd";

        public string Title;
        public ContentType Component;
        public string Extra;
        public GUIStyle EditorStyles = default;
        public GUILayoutOption[] LayoutOptions = null;

        public string FieldName;
        public Type Type;
        public string[] EnumOptions;
        public bool IsShow = true;
        public Vector3 ScrollPos;
    }

    internal class ContentLayout
    {
        [NonSerialized]
        public GUIStyle EditorStyles = default;
        [NonSerialized]
        public GUILayoutOption[] LayoutOptions = null;

        public ContentLayout(params GUILayoutOption[] layoutOptions)
        {
            LayoutOptions = layoutOptions;
        }

        public void SetStyles(GUIStyle style)
        {
            EditorStyles = style;
        }
    }
}