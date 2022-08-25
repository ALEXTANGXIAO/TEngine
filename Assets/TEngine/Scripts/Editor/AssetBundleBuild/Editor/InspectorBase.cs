using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TEngineCore.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TEngineCore.Editor
{
    public class InspectorBase : UnityEditor.Editor
    {

        private Dictionary<string, SerializedProperty> _serializedProperties = new Dictionary<string, SerializedProperty>();

        List<EditorContent> coms = new List<EditorContent>();

        private Dictionary<string, EditorContent> _comDic = new Dictionary<string, EditorContent>();

        private SoEditorBase _baseTarget;

        private bool _hasDispose;

        protected virtual void OnEnable()
        {
            SetTarget((SoEditorBase)serializedObject.targetObject);
        }

        protected void SetTarget(SoEditorBase target)
        {
            _baseTarget = target;
        }

        void Init()
        {
            _baseTarget.AdditionalInit();
            var type = _baseTarget.GetType().GetTypeInfo();
            _serializedProperties = new Dictionary<string, SerializedProperty>();
            ContentLayout layout = null;
            foreach (var member in type.DeclaredMembers)
            {
                var fie = type.GetDeclaredField(member.Name);
                if (fie != null)
                {
                    if (fie.FieldType.IsAssignableFrom(typeof(ContentLayout)))
                    {
                        layout = (ContentLayout)fie.GetValue(_baseTarget);
                        coms[coms.Count - 1].EditorStyles = layout.EditorStyles;
                        coms[coms.Count - 1].LayoutOptions = layout.LayoutOptions;
                        layout = null;
                    }

                    foreach (var att in member.GetCustomAttributes(false))
                    {
                        if (att is BuilderEditorAttribute a)
                        {
                            if (a.Content != null)
                            {
                                _serializedProperties.Add(member.Name, serializedObject.FindProperty(member.Name));
                                coms.Add(new EditorContent()
                                {
                                    Title = a.Content.Title,
                                    Component = a.Content.Component,
                                    Extra = a.Content.Extra,

                                    FieldName = member.Name,
                                });
                                coms[coms.Count - 1].Type = type.GetDeclaredField(member.Name).FieldType;
                                if (coms[coms.Count - 1].Type.IsSubclassOf(typeof(Enum)))
                                {
                                    List<string> enums = new List<string>();
                                    var box = coms[coms.Count - 1].Type.GetTypeInfo().DeclaredMembers;
                                    bool skipFirst = true;
                                    foreach (var field in box)
                                    {
                                        if (skipFirst)
                                        {
                                            skipFirst = false;
                                            continue;
                                        }

                                        enums.Add(field.Name);
                                    }

                                    coms[coms.Count - 1].EnumOptions = enums.ToArray();
                                    continue;
                                }
                                if (!coms[coms.Count - 1].Type.IsSubclassOf(typeof(Enum)) && a.Content.Component == ContentType.Enum)
                                {
                                    Debug.LogError(coms[coms.Count - 1].FieldName + "的类型非枚举！ 已剔除");
                                    coms.RemoveAt(coms.Count - 1);
                                }
                            }
                        }
                    }
                }
            }

            foreach (var com in coms)
            {
                _comDic.Add(com.FieldName, com);
            }

        }

        private int saveCount = 0;
        private const int saveCriticalPoint = 1000;
        private bool needSave = false;
        private bool canDraw = false;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (coms == null || coms.Count == 0)
            {
                Init();
                return;
            }

            if (CheckIfDispose())
                return;

            needSave = false;
            for (int i = 0; i < coms.Count; ++i)
            {
                var extra = coms[i].Extra.Split(',');

                bool canShow = true;
                string cbName = null;
                for (int j = 0; j < extra.Length; ++j)
                {
                    if (extra[j].StartsWith("Hori"))
                        continue;
                    if (extra[j].StartsWith("CB:"))
                    {
                        cbName = extra[j].Substring(3);
                    }
                    else if (extra[j].StartsWith("Flow"))
                    {
                        if (!canShow)
                            continue;

                        bool isFlowShow = extra[j].StartsWith("FlowS");
                        extra[j] = extra[j].Substring(6);
                        if (extra[j].Contains("|"))
                        {
                            var parents = extra[j].Split('|');
                            foreach (var parentInfo in parents)
                            {
                                canShow = CheckIfCanShowByParentInfo(parentInfo, isFlowShow);

                                if (canShow)
                                    break;
                            }
                        }
                        else
                        {
                            var parents = extra[j].Split('&');
                            foreach (var parentInfo in parents)
                            {
                                canShow = CheckIfCanShowByParentInfo(parentInfo, isFlowShow);
                                if (!canShow)
                                    break;
                            }
                        }
                    }
                }

                coms[i].IsShow = canShow;

                if (UnityEngine.Event.current.type == EventType.Layout)
                {
                    canDraw = true;
                }
                if (!canDraw)
                    return;
                bool horizontalStart = extra.Length > 0 && extra[0].Equals(EditorContent.HorizontalStart);
                bool horizontalEnd = extra.Length > 0 && extra[0].Equals(EditorContent.HorizontalEnd);
                if (horizontalStart)
                    EditorGUILayout.BeginHorizontal();

                if (canShow)
                {
                    DisplayComponets(i, cbName, ref needSave);
                }

                if (horizontalEnd)
                    EditorGUILayout.EndHorizontal();

                if (canShow)
                    GUILayout.Space(5);

            }

            saveCount++;

            if (needSave)
            {
                FunctionCall("OnAnyThingChange", "");
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
        }

        private void DisplayComponets(int i, string cbName, ref bool needSave)
        {

            switch (coms[i].Component)
            {
                case ContentType.Space:
                    float space = (float)Convert.ToDouble(coms[i].Title);
                    GUILayout.Space(space);
                    break;
                case ContentType.Label:
                    string valueLabel = coms[i].Title;
                    if (string.IsNullOrEmpty(valueLabel))
                    {
                        valueLabel = _serializedProperties[coms[i].FieldName].stringValue;
                    }

                    if (coms[i].EditorStyles != null && coms[i].LayoutOptions != null)
                        GUILayout.Label(valueLabel, coms[i].EditorStyles, coms[i].LayoutOptions);
                    else if (coms[i].EditorStyles != null)
                        GUILayout.Label(valueLabel, coms[i].EditorStyles);
                    else if (coms[i].LayoutOptions != null)
                        GUILayout.Label(valueLabel, coms[i].LayoutOptions);
                    else
                        GUILayout.Label(valueLabel);
                    break;
                case ContentType.SelectableLabel:
                    string valueSLabel = coms[i].Title;
                    if (string.IsNullOrEmpty(valueSLabel))
                    {
                        valueSLabel = _serializedProperties[coms[i].FieldName].stringValue;
                    }

                    if (coms[i].EditorStyles != null && coms[i].LayoutOptions != null)
                        EditorGUILayout.SelectableLabel(valueSLabel, coms[i].EditorStyles, coms[i].LayoutOptions);
                    else if (coms[i].EditorStyles != null)
                        EditorGUILayout.SelectableLabel(valueSLabel, coms[i].EditorStyles);
                    else if (coms[i].LayoutOptions != null)
                        EditorGUILayout.SelectableLabel(valueSLabel, coms[i].LayoutOptions);
                    else
                        EditorGUILayout.SelectableLabel(valueSLabel);
                    break;
                case ContentType.Button:
                    bool click = false;
                    if (coms[i].EditorStyles != null && coms[i].LayoutOptions != null)
                        click = GUILayout.Button(coms[i].Title, coms[i].EditorStyles, coms[i].LayoutOptions);
                    else if (coms[i].EditorStyles != null)
                        click = GUILayout.Button(coms[i].Title, coms[i].EditorStyles);
                    else if (coms[i].LayoutOptions != null)
                        click = GUILayout.Button(coms[i].Title, coms[i].LayoutOptions);
                    else
                        click = GUILayout.Button(coms[i].Title);

                    if (click)
                    {
                        //通知
                        if (!string.IsNullOrEmpty(cbName))
                            FunctionCall(cbName, "Button");
                    }

                    break;
                case ContentType.Enum:
                    EditorGUI.BeginChangeCheck();
                    if (coms[i].EditorStyles != null && coms[i].LayoutOptions != null)
                        _serializedProperties[coms[i].FieldName].intValue = EditorGUILayout.Popup(coms[i].Title, _serializedProperties[coms[i].FieldName].intValue, coms[i].EnumOptions, coms[i].EditorStyles, coms[i].LayoutOptions);
                    else if (coms[i].EditorStyles != null)
                        _serializedProperties[coms[i].FieldName].intValue = EditorGUILayout.Popup(coms[i].Title, _serializedProperties[coms[i].FieldName].intValue, coms[i].EnumOptions, coms[i].EditorStyles);
                    else if (coms[i].LayoutOptions != null)
                        _serializedProperties[coms[i].FieldName].intValue = EditorGUILayout.Popup(coms[i].Title, _serializedProperties[coms[i].FieldName].intValue, coms[i].EnumOptions, coms[i].LayoutOptions);
                    else
                        _serializedProperties[coms[i].FieldName].intValue = EditorGUILayout.Popup(coms[i].Title, _serializedProperties[coms[i].FieldName].intValue, coms[i].EnumOptions);
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        needSave = true;
                        //通知

                        if (!string.IsNullOrEmpty(cbName))
                            FunctionCall(cbName, coms[i].EnumOptions[_serializedProperties[coms[i].FieldName].enumValueIndex]);
                    }

                    break;
                case ContentType.Toggle:
                    EditorGUI.BeginChangeCheck();
                    if (coms[i].EditorStyles != null && coms[i].LayoutOptions != null)
                        _serializedProperties[coms[i].FieldName].boolValue = EditorGUILayout.Toggle(coms[i].Title, _serializedProperties[coms[i].FieldName].boolValue, coms[i].EditorStyles, coms[i].LayoutOptions);
                    else if (coms[i].EditorStyles != null)
                        _serializedProperties[coms[i].FieldName].boolValue = EditorGUILayout.Toggle(coms[i].Title, _serializedProperties[coms[i].FieldName].boolValue, coms[i].EditorStyles);
                    else if (coms[i].LayoutOptions != null)
                        _serializedProperties[coms[i].FieldName].boolValue = EditorGUILayout.Toggle(coms[i].Title, _serializedProperties[coms[i].FieldName].boolValue, coms[i].LayoutOptions);
                    else
                        _serializedProperties[coms[i].FieldName].boolValue = EditorGUILayout.Toggle(coms[i].Title, _serializedProperties[coms[i].FieldName].boolValue);

                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        needSave = true;
                        //通知

                        if (!string.IsNullOrEmpty(cbName))
                            FunctionCall(cbName, _serializedProperties[coms[i].FieldName].boolValue ? "1" : "0");
                    }

                    break;
                case ContentType.TextField:
                    EditorGUI.BeginChangeCheck();

                    if (coms[i].EditorStyles != null && coms[i].LayoutOptions != null)
                        _serializedProperties[coms[i].FieldName].stringValue = EditorGUILayout.TextField(coms[i].Title, _serializedProperties[coms[i].FieldName].stringValue, coms[i].EditorStyles, coms[i].LayoutOptions);
                    else if (coms[i].EditorStyles != null)
                        _serializedProperties[coms[i].FieldName].stringValue = EditorGUILayout.TextField(coms[i].Title, _serializedProperties[coms[i].FieldName].stringValue, coms[i].EditorStyles);
                    else if (coms[i].LayoutOptions != null)
                        _serializedProperties[coms[i].FieldName].stringValue = EditorGUILayout.TextField(coms[i].Title, _serializedProperties[coms[i].FieldName].stringValue, coms[i].LayoutOptions);
                    else
                        _serializedProperties[coms[i].FieldName].stringValue = EditorGUILayout.TextField(coms[i].Title, _serializedProperties[coms[i].FieldName].stringValue);

                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        needSave = true;
                        //通知

                        if (!string.IsNullOrEmpty(cbName))
                            FunctionCall(cbName, _serializedProperties[coms[i].FieldName].stringValue);
                    }

                    break;
                case ContentType.Obj:
                    EditorGUI.BeginChangeCheck();
                    Object obj = null;
                    if (_serializedProperties[coms[i].FieldName] != null)
                        obj = _serializedProperties[coms[i].FieldName].objectReferenceValue;
                    if (coms[i].LayoutOptions != null)
                        _serializedProperties[coms[i].FieldName].objectReferenceValue =
                            EditorGUILayout.ObjectField(coms[i].Title, _serializedProperties[coms[i].FieldName].objectReferenceValue, coms[i].Type, false, coms[i].LayoutOptions);
                    else
                        obj = EditorGUILayout.ObjectField(coms[i].Title, obj, coms[i].Type, false);
                    if (obj != null)
                    {
                        _serializedProperties[coms[i].FieldName].objectReferenceValue = obj;
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        needSave = true;
                        //通知

                        if (!string.IsNullOrEmpty(cbName))
                            FunctionCall(cbName, _serializedProperties[coms[i].FieldName].stringValue);
                    }
                    break;
                case ContentType.ScrollLabel:
                    string valueSCLabel = coms[i].Title;
                    if (string.IsNullOrEmpty(valueSCLabel))
                    {
                        valueSCLabel = _serializedProperties[coms[i].FieldName].stringValue;
                    }
                    if (coms[i].LayoutOptions != null)
                        coms[i].ScrollPos = GUILayout.BeginScrollView(coms[i].ScrollPos, false, false, coms[i].LayoutOptions);
                    else
                        coms[i].ScrollPos = GUILayout.BeginScrollView(coms[i].ScrollPos, false, false);

                    if (coms[i].EditorStyles != null && coms[i].LayoutOptions != null)
                    {
                        GUILayout.Label(valueSCLabel, coms[i].EditorStyles, coms[i].LayoutOptions);
                    }
                    else if (coms[i].EditorStyles != null)
                        GUILayout.Label(valueSCLabel, coms[i].EditorStyles);
                    else if (coms[i].LayoutOptions != null)
                        GUILayout.Label(valueSCLabel, coms[i].LayoutOptions);
                    else
                        GUILayout.Label(valueSCLabel);

                    GUILayout.EndScrollView();
                    break;
                default:
                    Debug.LogError("请勿使用不支持的类型，字段：" + coms[i].FieldName);
                    break;
            }
        }


        private void FunctionCall(string cbName, string parameter)
        {
            if (_baseTarget != null)
            {
                var ty = _baseTarget.GetType();
                var method = ty.GetTypeInfo().GetDeclaredMethod(cbName);
                if (method != null)
                {
                    var parameters = new object[] { parameter };
                    method.Invoke(_baseTarget, parameters);
                }
            }
        }

        private bool CheckIfCanShowByParentInfo(string parentInfo, bool checkShow)
        {
            if (CheckIfDispose())
                return false;

            var p2v = parentInfo.Split(':');

            if (p2v.Length < 2)
            {
                Debug.LogError("不规范的输入，参数后需包含     ‘:值’     :" + parentInfo);
                return false;
            }

            if (_comDic.TryGetValue(p2v[0], out var parent))
            {
                if (!checkShow)
                {
                    var or = p2v[1].Split('r');
                    foreach (var index in or)
                    {
                        if (index.Equals(_serializedProperties[p2v[0]].intValue.ToString()))
                            return true;
                    }
                }
                else
                {
                    return parent.IsShow;
                }
            }

            return false;
        }

        bool CheckIfDispose()
        {
            if (_hasDispose)
            {
                Selection.activeObject = null;
            }
            return _hasDispose;
        }

        private void OnValidate()
        {
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        void OnDisable()
        {
            _hasDispose = true;
        }
    }
}