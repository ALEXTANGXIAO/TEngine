using UnityEditor;
using UnityEngine;

namespace TEngine.Editor
{
    public static class LubanTools
    {
        [MenuItem("TEngine/Tools/Luban 转表")]
        public static void BuildLubanExcel()
        {
            Application.OpenURL(Application.dataPath + @"/../../Configs/GameConfig/gen_code_bin_to_project_lazyload.bat");
        }
        
        [MenuItem("TEngine/Tools/打开表格目录")]
        public static void OpenConfigFolder()
        {
            OpenFolderHelper.Execute(Application.dataPath + @"/../../Configs/GameConfig");
        }
    }
}