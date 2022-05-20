using UnityEditor;
using UnityEngine;

namespace Huatuo.Editor
{
    internal class HTEditorMenu
    {
        [MenuItem("TEngine/HuaTuo/安装升级HuaTuo", false, 3)]
        public static void ShowManager()
        {
            var win = HTEditorManger.GetWindow<HTEditorManger>(true);
            win.titleContent = new GUIContent("Huatuo Manager");
            win.ShowUtility();
        }
    }
}
