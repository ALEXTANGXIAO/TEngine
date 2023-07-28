using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TEngine.Editor
{
    public static class GenTools
    {
        public const string GenNetFirstTips = "ProtoGenTools.GenNetFirstTips";


        [MenuItem("TEngine/导出网络Proto|Gen Proto", false, 100)]
        public static void ExportProto()
        {
            var firstTips = EditorPrefs.GetBool(GenNetFirstTips, false);
            if (!firstTips && EditorUtility.DisplayDialog("提示", "导出网络Proto依赖于DotNet下Server解决方案！首次打开请编译！", "不再提示", "继续"))
            {
                EditorPrefs.SetBool(GenNetFirstTips, true);
                return;
            }
            Application.OpenURL(System.IO.Path.Combine(Application.dataPath,"../DotNet/start_export.bat"));
            Debug.Log("proto2cs succeed!");
        }
        
            

        [MenuItem("TEngine/导出Config|Export Config", false, 100)]
        public static void ExportConfig()
        {
            Application.OpenURL(System.IO.Path.Combine(Application.dataPath,"../Luban/gen_code_bin_to_project.bat"));
            Debug.Log("proto2cs succeed!");
        }
    }
}