using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TEngine.Editor.UI
{
    public class ScriptGenerator
    {
        private const string Gap = "/";

        [MenuItem("GameObject/ScriptGenerator/UIProperty", priority = 41)]
        public static void MemberProperty()
        {
            Generate(false);
        }

        [MenuItem("GameObject/ScriptGenerator/UIProperty - UniTask", priority = 43)]
        public static void MemberPropertyUniTask()
        {
            Generate(false, true);
        }

        [MenuItem("GameObject/ScriptGenerator/UIPropertyAndListener", priority = 42)]
        public static void MemberPropertyAndListener()
        {
            Generate(true);
        }

        [MenuItem("GameObject/ScriptGenerator/UIPropertyAndListener - UniTask", priority = 44)]
        public static void MemberPropertyAndListenerUniTask()
        {
            Generate(true, true);
        }

        private static void Generate(bool includeListener, bool isUniTask = false)
        {
            var root = Selection.activeTransform;
            if (root != null)
            {
                StringBuilder strVar = new StringBuilder();
                StringBuilder strBind = new StringBuilder();
                StringBuilder strOnCreate = new StringBuilder();
                StringBuilder strCallback = new StringBuilder();
                Ergodic(root, root, ref strVar, ref strBind, ref strOnCreate, ref strCallback, isUniTask);
                StringBuilder strFile = new StringBuilder();

                if (includeListener)
                {
#if ENABLE_TEXTMESHPRO
                    strFile.Append("using TMPro;\n");
#endif
                    if (isUniTask)
                    {
                        strFile.Append("using Cysharp.Threading.Tasks;\n");
                    }

                    strFile.Append("using UnityEngine;\n");
                    strFile.Append("using UnityEngine.UI;\n");
                    strFile.Append("using TEngine;\n\n");
                    strFile.Append($"namespace {SettingsUtils.GetUINameSpace()}\n");
                    strFile.Append("{\n");
                    strFile.Append("\t[Window(UILayer.UI)]\n");
                    strFile.Append("\tclass " + root.name + " : UIWindow\n");
                    strFile.Append("\t{\n");
                }

                // 脚本工具生成的代码
                strFile.Append("\t\t#region 脚本工具生成的代码\n");
                strFile.Append(strVar);
                strFile.Append("\t\tprotected override void ScriptGenerator()\n");
                strFile.Append("\t\t{\n");
                strFile.Append(strBind);
                strFile.Append(strOnCreate);
                strFile.Append("\t\t}\n");
                strFile.Append("\t\t#endregion");

                if (includeListener)
                {
                    strFile.Append("\n\n");
                    // #region 事件
                    strFile.Append("\t\t#region 事件\n");
                    strFile.Append(strCallback);
                    strFile.Append("\t\t#endregion\n\n");

                    strFile.Append("\t}\n");
                    strFile.Append("}\n");
                }

                TextEditor te = new TextEditor();
                te.text = strFile.ToString();
                te.SelectAll();
                te.Copy();
            }
            UnityEngine.Debug.Log($"脚本已生成到剪贴板，请自行Ctl+V粘贴");
        }

        public static void Ergodic(Transform root, Transform transform, ref StringBuilder strVar, ref StringBuilder strBind, ref StringBuilder strOnCreate,
            ref StringBuilder strCallback, bool isUniTask)
        {
            for (int i = 0; i < transform.childCount; ++i)
            {
                Transform child = transform.GetChild(i);
                WriteScript(root, child, ref strVar, ref strBind, ref strOnCreate, ref strCallback, isUniTask);
                if (child.name.StartsWith("m_item"))
                {
                    // 子 Item 不再往下遍历
                    continue;
                }

                Ergodic(root, child, ref strVar, ref strBind, ref strOnCreate, ref strCallback, isUniTask);
            }
        }

        private static string GetRelativePath(Transform child, Transform root)
        {
            StringBuilder path = new StringBuilder();
            path.Append(child.name);
            while (child.parent != null && child.parent != root)
            {
                child = child.parent;
                path.Insert(0, Gap);
                path.Insert(0, child.name);
            }

            return path.ToString();
        }

        public static string GetBtnFuncName(string varName)
        {
            return "OnClick" + varName.Replace("m_btn", string.Empty) + "Btn";
        }

        public static string GetToggleFuncName(string varName)
        {
            return "OnToggle" + varName.Replace("m_toggle", string.Empty) + "Change";
        }

        public static string GetSliderFuncName(string varName)
        {
            return "OnSlider" + varName.Replace("m_slider", string.Empty) + "Change";
        }

        private static void WriteScript(Transform root, Transform child, ref StringBuilder strVar, ref StringBuilder strBind, ref StringBuilder strOnCreate,
            ref StringBuilder strCallback, bool isUniTask)
        {
            string varName = child.name;
            string componentName = string.Empty;

            var rule = SettingsUtils.GetScriptGenerateRule().Find(t => varName.StartsWith(t.uiElementRegex));

            if (rule != null)
            {
                componentName = rule.componentName;
            }

            if (componentName == string.Empty)
            {
                return;
            }

            string varPath = GetRelativePath(child, root);
            if (!string.IsNullOrEmpty(varName))
            {
                strVar.Append("\t\tprivate " + componentName + " " + varName + ";\n");
                switch (componentName)
                {
                    case "Transform":
                        strBind.Append($"\t\t\t{varName} = FindChild(\"{varPath}\");\n");
                        break;
                    case "GameObject":
                        strBind.Append($"\t\t\t{varName} = FindChild(\"{varPath}\").gameObject;\n");
                        break;
                    case "AnimationCurve":
                        strBind.Append($"\t\t\t{varName} = FindChildComponent<AnimCurveObject>(\"{varPath}\").m_animCurve;\n");
                        break;
                    case "RichItemIcon":
                    case "CommonFightWidget":
                    case "PlayerHeadWidget":
                        strBind.Append($"\t\t\t{varName} = CreateWidgetByType<{componentName}>(\"{varPath}\");\n");
                        break;
                    case "RedNoteBehaviour":
                    case "TextButtonItem":
                    case "SwitchTabItem":
                    case "UIActorWidget":
                    case "UIEffectWidget":
                    case "UISpineWidget":
                        strBind.Append($"\t\t\t{varName} = CreateWidget<{componentName}>(\"{varPath}\");\n");
                        break;
                    case "ActorNameBinderText":
                        strBind.Append($"\t\t\t{varName} = FindTextBinder(\"{varPath}\");\n");
                        break;
                    case "ActorNameBinderEffect":
                        strBind.Append($"\t\t\t{varName} = FindEffectBinder(\"{varPath}\");\n");
                        break;
                    default:
                        strBind.Append($"\t\t\t{varName} = FindChildComponent<{componentName}>(\"{varPath}\");\n");
                        break;
                }

                if (componentName == "Button")
                {
                    string varFuncName = GetBtnFuncName(varName);
                    if (isUniTask)
                    {
                        strOnCreate.Append($"\t\t\t{varName}.onClick.AddListener(UniTask.UnityAction({varFuncName}));\n");
                        strCallback.Append($"\t\tprivate async UniTaskVoid {varFuncName}()\n");
                        strCallback.Append("\t\t{\n await UniTask.Yield();\n\t\t}\n");
                    }
                    else
                    {
                        strOnCreate.Append($"\t\t\t{varName}.onClick.AddListener({varFuncName});\n");
                        strCallback.Append($"\t\tprivate void {varFuncName}()\n");
                        strCallback.Append("\t\t{\n\t\t}\n");
                    }
                }
                else if (componentName == "Toggle")
                {
                    string varFuncName = GetToggleFuncName(varName);
                    strOnCreate.Append($"\t\t\t{varName}.onValueChanged.AddListener({varFuncName});\n");
                    strCallback.Append($"\t\tprivate void {varFuncName}(bool isOn)\n");
                    strCallback.Append("\t\t{\n\t\t}\n");
                }
                else if (componentName == "Slider")
                {
                    string varFuncName = GetSliderFuncName(varName);
                    strOnCreate.Append($"\t\t\t{varName}.onValueChanged.AddListener({varFuncName});\n");
                    strCallback.Append($"\t\tprivate void {varFuncName}(float value)\n");
                    strCallback.Append("\t\t{\n\t\t}\n");
                }
            }
        }

        public class GeneratorHelper : EditorWindow
        {
            [MenuItem("GameObject/ScriptGenerator/About", priority = 49)]
            public static void About()
            {
                GeneratorHelper welcomeWindow = (GeneratorHelper)EditorWindow.GetWindow(typeof(GeneratorHelper), false, "About");
            }

            public void Awake()
            {
                minSize = new Vector2(400, 600);
            }

            protected void OnGUI()
            {
                GUILayout.BeginVertical();
                foreach (var item in SettingsUtils.GetScriptGenerateRule())
                {
                    GUILayout.Label(item.uiElementRegex + "：\t" + item.componentName);
                }

                GUILayout.EndVertical();
            }
        }

        public class SwitchGroupGenerator
        {
            private const string Condition = "m_switchGroup";

            public static readonly SwitchGroupGenerator Instance = new SwitchGroupGenerator();

            public string Process(Transform root)
            {
                var sbd = new StringBuilder();
                var list = new List<Transform>();
                Collect(root, list);
                foreach (var node in list)
                {
                    sbd.AppendLine(Process(root, node)).AppendLine();
                }

                return sbd.ToString();
            }

            public void Collect(Transform node, List<Transform> nodeList)
            {
                if (node.name.StartsWith(Condition))
                {
                    nodeList.Add(node);
                    return;
                }

                var childCnt = node.childCount;
                for (var i = 0; i < childCnt; i++)
                {
                    var child = node.GetChild(i);
                    Collect(child, nodeList);
                }
            }

            private string Process(Transform root, Transform groupTf)
            {
                var parentPath = GetPath(root, groupTf);
                var name = groupTf.name;
                var sbd = new StringBuilder(@"
var _namePath = ""#parentPath"";
var _nameTf = FindChild(_namePath);
var childCnt = _nameTf.childCount;
SwitchTabItem[] _name;
_name = new SwitchTabItem[childCnt];
for (var i = 0; i < childCnt; i++)
{
    var child = _nameTf.GetChild(i);
    _name[i] = CreateWidget<SwitchTabItem>(_namePath + ""/"" + child.name);
}");
                sbd.Replace("_name", name);
                sbd.Replace("#parentPath", parentPath);
                return sbd.ToString();
            }

            public string GetPath(Transform root, Transform childTf)
            {
                if (childTf == null)
                {
                    return string.Empty;
                }

                if (childTf == root)
                {
                    return childTf.name;
                }

                var parentPath = GetPath(root, childTf.parent);
                if (parentPath == string.Empty)
                {
                    return childTf.name;
                }

                return parentPath + "/" + childTf.name;
            }
        }
    }
}