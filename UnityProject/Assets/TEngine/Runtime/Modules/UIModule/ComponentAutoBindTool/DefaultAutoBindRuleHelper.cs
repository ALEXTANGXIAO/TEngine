#if false
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 默认自动绑定规则辅助器
/// </summary>
public class DefaultAutoBindRuleHelper : IAutoBindRuleHelper
{

    /// <summary>
    /// 命名前缀与类型的映射
    /// </summary>
    private Dictionary<string, string> m_PrefixesDict = new Dictionary<string, string>()
    {
        {"Trans","Transform" },
        {"OldAnim","Animation"}, 
        {"NewAnim","Animator"},

        {"Rect","RectTransform"},
        {"Canvas","Canvas"},
        {"Group","CanvasGroup"},
        {"VGroup","VerticalLayoutGroup"},
        {"HGroup","HorizontalLayoutGroup"},
        {"GGroup","GridLayoutGroup"},
        {"TGroup","ToggleGroup"},

        {"Btn","UIButtonSuper"},
        {"Img","Image"},
        {"RImg","RawImage"},
        {"Txt","Text"},
        {"TxtM","TextMeshProUGUI"},
        {"Input","TMP_InputField"},
        {"Slider","Slider"},
        {"Mask","Mask"},
        {"Mask2D","RectMask2D"},
        {"Tog","Toggle"},
        {"Sbar","Scrollbar"},
        {"SRect","ScrollRect"},
        {"Drop","Dropdown"},
        {"USpriteAni","UGUISpriteAnimation"},
        {"VGridV","LoopGridView"},
        {"HGridV","LoopGridView"},
        {"VListV","LoopListView2"},
        {"HListV","LoopListView2"},
    };

    public bool IsValidBind( Transform target, List<string> filedNames, List<string> componentTypeNames)
    {
        string[] strArray = target.name.Split('_');

        if (strArray.Length == 1)
        {
            return false;
        }

        string filedName = strArray[^1];

        for (int i = 0; i < strArray.Length - 1; i++)
        {
            string str = strArray[i];
            if (m_PrefixesDict.TryGetValue(str,out var componentName))
            {
                filedNames.Add($"{str}_{filedName}");
                componentTypeNames.Add(componentName);
            }
            else
            {
                Debug.LogError($"{target.name}的命名中{str}不存在对应的组件类型，绑定失败");
                return false;
            }
        }

        return true;
    }
}
#endif
