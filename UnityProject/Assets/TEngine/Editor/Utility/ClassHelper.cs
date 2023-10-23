using UnityEditor;
using UnityEngine;

/// <summary>
/// Unity编辑器类帮助类。
/// </summary>
public static class ClassHelper
{
    /// <summary>
    /// 获取MonoBehaviour的脚本Id。
    /// </summary>
    /// <param name="type">脚本类型。</param>
    /// <returns>脚本Id。</returns>
    public static int GetClassID(System.Type type)
    {
        GameObject gameObject = EditorUtility.CreateGameObjectWithHideFlags("Temp", HideFlags.HideAndDontSave);
        Component component = gameObject.AddComponent(type);
        SerializedObject @class = new SerializedObject(component);
        int classID = @class.FindProperty("m_Script").objectReferenceInstanceIDValue;
        Object.DestroyImmediate(gameObject);
        return classID;
    }

    /// <summary>
    /// 获取MonoBehaviour的脚本Id。
    /// </summary>
    /// <typeparam name="T">脚本类型。</typeparam>
    /// <returns>脚本Id。</returns>
    public static int GetClassID<T>() where T : MonoBehaviour
    {
        return GetClassID(typeof(T));
    }

    #region Method Documentation
    /************************************************************************************************************
    Example:
            [MenuItem("GameObject/UI/转化成CustomText", false, 1999)]
            public static void ConvertToCustomText(MenuCommand menuCommand)
            {
                GameObject go = menuCommand.context as GameObject;
                if (go != null)
                {
                    Text text = go.GetComponent<Text>();
                    if (text != null)
                    {
                        var ob = ClassHelper.ReplaceClass(text, typeof(CustomText));
                        ob.ApplyModifiedProperties();
                    }
                }
            }
    ************************************************************************************************************/
    #endregion
    /// <summary>
    /// 替换MonoBehaviour脚本。
    /// </summary>
    /// <param name="monoBehaviour"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static SerializedObject ReplaceClass(MonoBehaviour monoBehaviour, System.Type type)
    {
        int classID = GetClassID(type);
        SerializedObject @class = new SerializedObject(monoBehaviour);
        @class.Update();
        @class.FindProperty("m_Script").objectReferenceInstanceIDValue = classID;
        @class.ApplyModifiedProperties();
        @class.Update();
        return @class;
    }
}