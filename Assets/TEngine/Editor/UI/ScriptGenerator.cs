using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TEngine.Editor.UI
{
    public class ScriptGenerator
    {
        private const string Gap = "/";

        [MenuItem("GameObject/ScriptGenerator/UIProperty", priority = 49)]
        public static void MemberProperty()
        {
            Generate(false);
        }

        [MenuItem("GameObject/ScriptGenerator/UIProperty UniTask", priority = 49)]
        public static void MemberPropertyUniTask()
        {
            Generate(false, true);
        }

        [MenuItem("GameObject/ScriptGenerator/UIPropertyAndListener", priority = 49)]
        public static void MemberPropertyAndListener()
        {
            Generate(true);
        }

        [MenuItem("GameObject/ScriptGenerator/UIPropertyAndListener UniTask", priority = 49)]
        public static void MemberPropertyAndListenerUniTask()
        {
            Generate(true, true);
        }

        [MenuItem("GameObject/ScriptGenerator/UISwitchGroup", priority = 49)]
        public static void UISwitchGroup()
        {
            var root = Selection.activeTransform;
            if (root == null)
            {
                return;
            }

            var content = SwitchGroupGenerator.Instance.Process(root);
            TextEditor te = new TextEditor();
            te.text = content;
            te.SelectAll();
            te.Copy();
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
                    strFile.Append("using UnityGameFramework.Runtime;\n\n");
                    strFile.Append("namespace GameMain\n");
                    strFile.Append("{\n");
                    strFile.Append("\tclass " + root.name + " : UIFormLogic\n");
                    strFile.Append("\t{\n");
                }

                // 脚本工具生成的代码
                strFile.Append("\t\t#region 脚本工具生成的代码\n");
                strFile.Append(strVar);
                strFile.Append("\t\tpublic override void ScriptGenerator()\n");
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
        }

        private static void Ergodic(Transform root, Transform transform, ref StringBuilder strVar, ref StringBuilder strBind, ref StringBuilder strOnCreate,
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

        private static string GetBtnFuncName(string varName)
        {
            return "OnClick" + varName.Replace("m_btn", string.Empty) + "Btn";
        }

        private static string GetToggleFuncName(string varName)
        {
            return "OnToggle" + varName.Replace("m_toggle", string.Empty) + "Change";
        }

        private static string GetSliderFuncName(string varName)
        {
            return "OnSlider" + varName.Replace("m_slider", string.Empty) + "Change";
        }

        public static Dictionary<string, string> dicWidget = new Dictionary<string, string>()
        {
            { "m_go", "GameObject" },
            { "m_item", "GameObject" },
            { "m_tf", "Transform" },
            { "m_rect", "RectTransform" },
#if ENABLE_TEXTMESHPRO
        {"m_text","TextMeshProUGUI"},
#else
            { "m_text", "Text" },
#endif
            { "m_richText", "RichTextItem" },
            { "m_tbtn", "TextButtonItem" },
            { "m_btn", "Button" },
            { "m_img", "Image" },
            { "m_rimg", "RawImage" },
            { "m_scrollBar", "Scrollbar" },
            { "m_scroll", "ScrollRect" },
            { "m_input", "InputField" },
            { "m_grid", "GridLayoutGroup" },
            { "m_clay", "CircleLayoutGroup" },
            { "m_hlay", "HorizontalLayoutGroup" },
            { "m_vlay", "VerticalLayoutGroup" },
            { "m_red", "RedNoteBehaviour" },
            { "m_switch", "SwitchTabItem" },
            { "m_slider", "Slider" },
            { "m_group", "ToggleGroup" },
            { "m_curve", "AnimationCurve" },
            { "m_fightWidget", "CommonFightWidget" },
            { "m_canvasGroup", "CanvasGroup" },
#if ENABLE_TEXTMESHPRO
        {"m_tmp","TextMeshProUGUI"},
#endif
        };

        private static void WriteScript(Transform root, Transform child, ref StringBuilder strVar, ref StringBuilder strBind, ref StringBuilder strOnCreate,
            ref StringBuilder strCallback, bool isUniTask)
        {
            string varName = child.name;
            string varType = string.Empty;
            foreach (var pair in dicWidget)
            {
                var key = pair.Key;
                if (varName.StartsWith(key))
                {
                    varType = pair.Value;
                }
            }

            if (varType == string.Empty)
            {
                return;
            }

            string varPath = GetRelativePath(child, root);
            if (!string.IsNullOrEmpty(varName))
            {
                strVar.Append("\t\tprivate " + varType + " " + varName + ";\n");
                switch (varType)
                {
                    case "Transform":
                        strBind.Append(string.Format("\t\t\t{0} = FindChild(\"{1}\");\n", varName, varPath));
                        break;
                    case "GameObject":
                        strBind.Append(string.Format("\t\t\t{0} = FindChild(\"{1}\").gameObject;\n", varName, varPath));
                        break;
                    case "AnimationCurve":
                        strBind.Append(string.Format("\t\t\t{0} = FindChildComponent<AnimCurveObject>(\"{1}\").m_animCurve;\n", varName, varPath));
                        break;
                    case "RichItemIcon":
                    case "CommonFightWidget":
                    case "PlayerHeadWidget":
                        strBind.Append(string.Format("\t\t\t{0} = CreateWidgetByType<{1}>(\"{2}\");\n", varName, varType, varPath));
                        break;
                    case "RedNoteBehaviour":
                    case "TextButtonItem":
                    case "SwitchTabItem":
                    case "UIActorWidget":
                    case "UIEffectWidget":
                    case "UISpineWidget":
                        strBind.Append(string.Format("\t\t\t{0} = CreateWidget<{1}>(\"{2}\");\n", varName, varType, varPath));
                        break;
                    case "ActorNameBinderText":
                        strBind.Append(string.Format("\t\t\t{0} = FindTextBinder(\"{1}\");\n", varName, varPath));
                        break;
                    case "ActorNameBinderEffect":
                        strBind.Append(string.Format("\t\t\t{0} = FindEffectBinder(\"{1}\");\n", varName, varPath));
                        break;
                    default:
                        strBind.Append(string.Format("\t\t\t{0} = FindChildComponent<{1}>(\"{2}\");\n", varName, varType, varPath));
                        break;
                }

                if (varType == "Button")
                {
                    string varFuncName = GetBtnFuncName(varName);
                    if (isUniTask)
                    {
                        strOnCreate.Append(string.Format("\t\t\t{0}.onClick.AddListener(UniTask.UnityAction({1}));\n", varName, varFuncName));
                        strCallback.Append(string.Format("\t\tprivate async UniTaskVoid {0}()\n", varFuncName));
                        strCallback.Append("\t\t{\n await UniTask.Yield();\n\t\t}\n");
                    }
                    else
                    {
                        strOnCreate.Append(string.Format("\t\t\t{0}.onClick.AddListener({1});\n", varName, varFuncName));
                        strCallback.Append(string.Format("\t\tprivate void {0}()\n", varFuncName));
                        strCallback.Append("\t\t{\n\t\t}\n");
                    }
                }
                else if (varType == "Toggle")
                {
                    string varFuncName = GetToggleFuncName(varName);
                    strOnCreate.Append(string.Format("\t\t\t{0}.onValueChanged.AddListener({1});\n", varName, varFuncName));
                    strCallback.Append(string.Format("\t\tprivate void {0}(bool isOn)\n", varFuncName));
                    strCallback.Append("\t\t{\n\t\t}\n");
                }
                else if (varType == "Slider")
                {
                    string varFuncName = GetSliderFuncName(varName);
                    strOnCreate.Append(string.Format("\t\t\t{0}.onValueChanged.AddListener({1});\n", varName, varFuncName));
                    strCallback.Append(string.Format("\t\tprivate void {0}(float value)\n", varFuncName));
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
                foreach (var item in ScriptGenerator.dicWidget)
                {
                    GUILayout.Label(item.Key + "：\t" + item.Value);
                }
            }
        }

        public class SwitchGroupGeneratorHelper : EditorWindow
        {
            [MenuItem("GameObject/ScriptGenerator/AboutSwitchGroup", priority = 50)]
            public static void About()
            {
                GetWindow(typeof(SwitchGroupGeneratorHelper), false, "AboutSwitchGroup");
            }

            public void Awake()
            {
                minSize = new Vector2(400, 600);
            }

            protected void OnGUI()
            {
                GUILayout.BeginVertical();
                GUILayout.Label(SwitchGroupGenerator.CONDITION + "：\t" + "SwitchTabItem[]");
            }
        }

        public class SwitchGroupGenerator
        {
            /*
             遍历子节点，找到所有名为 m_switchGroup 开始的节点，输出该节点
             */

            public const string CONDITION = "m_switchGroup";

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
                if (node.name.StartsWith(CONDITION))
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

            public string Process(Transform root, Transform groupTf)
            {
                var parentPath = GetPath(root, groupTf);
                var _name = groupTf.name;
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
                sbd.Replace("_name", _name);
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