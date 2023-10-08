using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector.Editor;
using TEngine.Editor.UI;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace TEngine
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIElement), true)]
    public class UIElementEditor : OdinEditor
    {
        protected UIElement Element;

        protected override void OnEnable()
        {
            base.OnEnable();
            Element = target as UIElement;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            if (GUILayout.Button("Generate", GUILayout.Width(130)))
            {
                CheckUiItems();
            }

            serializedObject.ApplyModifiedProperties();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(Element);
            }

            base.OnInspectorGUI();
        }

        protected void CheckUiItems()
        {
            if (Element == null) return;
            ClearUnusedItems();
            var root = Element.transform;
            StringBuilder strVar = new StringBuilder();
            StringBuilder strBind = new StringBuilder();
            StringBuilder strOnCreate = new StringBuilder();
            StringBuilder strOnCreateRedNote = new StringBuilder();
            StringBuilder strCallback = new StringBuilder();
            Ergodic(root, root, ref strVar, ref strBind, ref strOnCreate, ref strOnCreateRedNote, ref strCallback);
            StringBuilder strFile = new StringBuilder();

            // 脚本工具生成的代码
            strFile.Append("\t\t#region 脚本工具生成的代码\n");
            strFile.Append(strVar);
            strFile.Append("\t\tpublic override void ScriptGenerator()\n");
            strFile.Append("\t\t{\n");
            strFile.Append("\t\t\tCheckUIElement();\n");
            strFile.Append(strBind);
            strFile.Append(strOnCreate);
            strFile.Append(strOnCreateRedNote);
            strFile.Append("\t\t}\n");
            strFile.Append("\t\t#endregion");

            TextEditor te = new TextEditor();
            te.text = strFile.ToString();
            te.SelectAll();
            te.Copy();
        }

        private void ClearUnusedItems()
        {
            if (Element == null || Element.Elements == null) return;
            List<string> ids = new List<string>();
            foreach (var kv in Element.Elements)
            {
                if (kv.Value == null || !string.IsNullOrEmpty(GetVerType(kv.Key)))
                {
                    ids.Add(kv.Key);
                }
            }

            foreach (var id in ids)
            {
                Element.Elements.Remove(id);
            }
        }

        private void Ergodic(Transform root, Transform transform, ref StringBuilder strVar, ref StringBuilder strBind, ref StringBuilder strOnCreate,
            ref StringBuilder strOnCreateRedNote, ref StringBuilder strCallback)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform child = transform.GetChild(i);
                WriteScript(root, child, ref strVar, ref strBind, ref strOnCreate, ref strOnCreateRedNote, ref strCallback);
                if (child.name.StartsWith("m_item"))
                {
                    continue;
                }

                Ergodic(root, child, ref strVar, ref strBind, ref strOnCreate, ref strOnCreateRedNote, ref strCallback);
            }
        }

        private void WriteScript(Transform root, Transform child, ref StringBuilder strVar, ref StringBuilder strBind, ref StringBuilder strOnCreate,
            ref StringBuilder strOnCreateRedNote, ref StringBuilder strCallback)
        {
            var varName = child.name;
            var varType = GetVerType(varName);
            if (varType == string.Empty) return;
            if (Element.Elements.Contains(varName))
            {
                Debug.LogError("有重复的key:" + varName);
                return;
            }

            Element.Elements[varName] = child;
            if (!string.IsNullOrEmpty(varName))
            {
                strVar.Append("\t\tprivate " + varType + " " + varName + ";\n");
                switch (varType)
                {
                    case "Transform":
                        strBind.Append($"\t\t\t{varName} = FChild(\"{varName}\");\n");
                        break;
                    case "GameObject":
                        strBind.Append($"\t\t\t{varName} = FChild(\"{varName}\").gameObject;\n");
                        break;
                    case "RichItemIcon":
                        strBind.Append($"\t\t\t{varName} = CreateWidgetByType<{varType}>(FChild(\"{varName}\"));\n");
                        break;
                    case "RedNoteWidget":
                        break;
                    case "TextButtonItem":
                    case "SwitchTabItem":
                    case "UIActorWidget":
                    case "UIEffectWidget":
                    case "UISpineWidget":
                    case "UIMainPlayerWidget":
                        strBind.Append($"\t\t\t{varName} = CreateWidget<{varType}>(FChild(\"{varName}\").gameObject);\n");
                        break;
                    default:
                        strBind.Append($"\t\t\t{varName} = FChild<{varType}>(\"{varName}\");\n");
                        break;
                }

                if (varType == "Button")
                {
                    string varFuncName = ScriptGenerator.GetBtnFuncName(varName);
                    strOnCreate.Append($"\t\t\t{varName}.onClick.AddListener({varFuncName});\n");
                    strCallback.Append($"\t\tprivate void {varFuncName}()\n");
                    strCallback.Append("\t\t{\n\t\t}\n");
                }
                else if (varType == "Toggle")
                {
                    string varFuncName = ScriptGenerator.GetToggleFuncName(varName);
                    strOnCreate.Append($"\t\t\t{varName}.onValueChanged.AddListener({varFuncName});\n");
                    strCallback.Append($"\t\tprivate void {varFuncName}(bool isOn)\n");
                    strCallback.Append("\t\t{\n\t\t}\n");
                }
            }
        }

        protected string GetVerType(string uiName)
        {
            foreach (var pair in SettingsUtils.GetScriptGenerateRule())
            {
                if (uiName.StartsWith(pair.uiElementRegex))
                {
                    return pair.componentName;
                }
            }

            return string.Empty;
        }
    }
}
#endif