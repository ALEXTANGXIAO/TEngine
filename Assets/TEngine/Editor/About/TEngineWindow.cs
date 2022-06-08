using UnityEditor;
using UnityEngine;

namespace TEngine.Editor
{
    public class TEngineWindow : EditorWindow
    {
        public enum ModuleType
        {
            /// <summary>
            /// 存在Package中
            /// </summary>
            InPackage,
            /// <summary>
            /// 存在项目中
            /// </summary>
            InProject
        }
        public static ModuleType showModuleType;

        [MenuItem("TEngine/管理面板|TEngine管理面板", priority = 1500)]
        private static void Open()
        {
            var window = GetWindow<TEngineWindow>("管理面板|TEngine管理面板");
            window.minSize = new Vector2(900, 600);
            window.maxSize = new Vector2(900, 600);
            window.Show();
        }

        private void OnEnable()
        {
            showModuleType = (ModuleType)EditorPrefs.GetInt("showModuleType", 0);

        }

        private void OnGUI()
        {
            //水平布局
            GUILayout.BeginHorizontal("Toolbar");
            {
                //搜索
                
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();


            GUILayout.EndHorizontal();

            //水平布局
            GUILayout.BeginHorizontal(GUILayout.ExpandHeight(true));
            {
                //垂直布局 设置左侧列表宽度
                GUILayout.BeginVertical(GUILayout.Width(200f));
                {
                    //绘制列表
                    
                }
                GUILayout.EndVertical();

                //分割线
                GUILayout.Box(string.Empty, "EyeDropperVerticalLine", GUILayout.ExpandHeight(true), GUILayout.Width(1f));

                //垂直布局
                GUILayout.BeginVertical(GUILayout.ExpandHeight(true));
                {
                    //绘制详情
                    
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
    }

}
