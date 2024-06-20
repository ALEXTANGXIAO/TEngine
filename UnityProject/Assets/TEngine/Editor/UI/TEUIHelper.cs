using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
#region
//作者:Saber
#endregion
namespace TEngine.Editor.UI
{
    public class TEUIHelper
    {
        
        public static string Generate(bool includeListener, bool isUniTask = false, Transform root = null, string nameSpace = "",string className="")
        {
            if (root != null)
            {
                StringBuilder strVar = new StringBuilder();
                StringBuilder strBind = new StringBuilder();
                StringBuilder strOnCreate = new StringBuilder();
                StringBuilder strCallback = new StringBuilder();

                ScriptGenerator.Ergodic(root, root, ref strVar, ref strBind, ref strOnCreate, ref strCallback, isUniTask);

                //object[] args = new object[] { root, root, strVar, strBind, strOnCreate, strCallback, isUniTask };
                //typeof(TEngine.Editor.UI.ScriptGenerator).GetMethod("Ergodic",System.Reflection.BindingFlags.NonPublic| System.Reflection.BindingFlags.Static).Invoke(null,args);
                //strVar = args[2] as StringBuilder;
                //strBind = args[3] as StringBuilder;
                //strOnCreate = args[4] as StringBuilder;
                //strCallback = args[5] as StringBuilder;

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
                }
                strFile.Append("using UnityEngine;\n");
                strFile.Append("using UnityEngine.UI;\n");
                strFile.Append("using TEngine;\n\n");
                nameSpace = string.IsNullOrEmpty(nameSpace) ? SettingsUtils.GetUINameSpace() : nameSpace;
                strFile.Append($"namespace {nameSpace}\n");
                strFile.Append("{\n");
                //strFile.Append("\t[Window(UILayer.UI)]\n");
                strFile.Append("\tpartial class " + className + "\n");
                strFile.Append("\t{\n");

                // 脚本工具生成的代码
                strFile.Append("\t\t#region 脚本工具生成的代码\n");
                strFile.Append(strVar);
                strFile.Append("\t\tprotected override void ScriptGenerator()\n");
                strFile.Append("\t\t{\n");
                strFile.Append(strBind);
                strFile.Append(strOnCreate);
                strFile.Append("\t\t}\n");
                strFile.Append("\t\t#endregion");

                strFile.Append("\n\t}");
                strFile.Append("\n}");
                return strFile.ToString();

            }
            return string.Empty;
        }

    }
}

