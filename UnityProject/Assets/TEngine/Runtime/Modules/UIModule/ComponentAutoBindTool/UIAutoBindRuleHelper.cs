using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI自动绑定规则辅助器
/// </summary>
public class UIAutoBindRuleHelper: IAutoBindRuleHelper
{
    public bool IsValidBind( Transform targetTransform, List<string> filedNames, List<string> componentTypeNames)
     {
         string uiElementName = targetTransform.name;
         string[] strArray = targetTransform.name.Split('_');

         if (strArray.Length == 1)
         {
             return false;
         }

         string filedName = strArray[^1];
         var rule = SettingsUtils.GetScriptGenerateRule().Find(t => uiElementName.StartsWith(t.uiElementRegex));

         if (rule != null)
         {
             filedNames.Add($"{filedName}");
             componentTypeNames.Add(rule.componentName);
             return true;
         }
         Debug.LogWarning($"{targetTransform.name}的命名中{uiElementName}不存在对应的组件类型，绑定失败");
         return false;
     }
}