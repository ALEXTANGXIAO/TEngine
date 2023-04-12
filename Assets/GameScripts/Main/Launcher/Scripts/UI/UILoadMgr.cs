using System.Collections.Generic;
using UnityEngine;
using TEngine;

namespace GameMain
{
    public static class UILoadMgr
    {
        private static GameObject _uiLoad;
        private static Dictionary<string, string> _uiList = new Dictionary<string, string>();
        private static readonly Dictionary<string, UIBase> _uiMap = new Dictionary<string, UIBase>();
        /// <summary>
        /// 初始化根节点
        /// </summary>
        public static void Initialize()
        {
            _uiLoad = GameObject.Find("AssetLoad");           
            if (_uiLoad == null)
            {
                var obj = Resources.Load($"AssetLoad/UILoad");
                if (obj == null)
                {
                    Log.Error("Failed to load UILoad. Please check the resource path");
                    return;
                }
                _uiLoad = Object.Instantiate(obj) as GameObject;
                if (_uiLoad != null)
                {
                    _uiLoad.name = "AssetLoad";
                    _uiLoad.transform.SetAsLastSibling();
                }
                else
                {
                    Log.Error($"AssetLoad object Instantiate Failed");
                    return;
                }
            }
            RegisterUI();
        }     
              
        public static void RegisterUI()
        {
            UIDefine.RegisterUI(_uiList);
        }

        /// <summary>
        /// show ui
        /// </summary>
        /// <param name="uiInfo">对应的ui</param>
        /// <param name="param">参数</param>
        public static void Show(string uiInfo,object param = null)
        {
            if (string.IsNullOrEmpty(uiInfo))
                return;

            if (!_uiList.ContainsKey(uiInfo))
            {
                Log.Error($"not define ui:{uiInfo}");
                return;
            }

            GameObject ui = null;
            if (!_uiMap.ContainsKey(uiInfo))
            {                
                Object obj = Resources.Load(_uiList[uiInfo]);
                if (obj != null)
                {
                    ui = Object.Instantiate(obj) as GameObject;
                    if (ui != null)
                    {
                        ui.transform.SetParent(_uiLoad.transform);
                        ui.transform.localScale = Vector3.one;
                        ui.transform.localPosition = Vector3.zero;
                        RectTransform rect = ui.GetComponent<RectTransform>();
                        rect.sizeDelta = Vector2.zero;
                    }                   
                }

                UIBase compenent = ui.GetComponent<UIBase>();
                if (compenent != null)
                {
                    _uiMap.Add(uiInfo, compenent);
                }                
            }
            _uiMap[uiInfo].gameObject.SetActive(true);
            if (param != null)
            {
                UIBase component = _uiMap[uiInfo].GetComponent<UIBase>();
                if (component != null)
                {
                    component.OnEnter(param);
                }
            }            
        }
        /// <summary>
        /// 隐藏ui对象
        /// </summary>
        /// <param name="uiinfo">对应的ui</param>
        public static void Hide(string uiinfo)
        {
            if (string.IsNullOrEmpty(uiinfo))
            {
                return;
            }

            if (!_uiMap.ContainsKey(uiinfo))
            {
                return;
            }

            _uiMap[uiinfo].gameObject.SetActive(false);
            Object.DestroyImmediate(_uiMap[uiinfo].gameObject);
            _uiMap.Remove(uiinfo);            
        }

        /// <summary>
        /// 获取显示的ui对象
        /// </summary>
        /// <param name="ui"></param>
        /// <returns></returns>
        public static UIBase GetActiveUI(string ui)
        {
            return _uiMap.ContainsKey(ui) ? _uiMap[ui] : null;
        }

        /// <summary>
        /// 隐藏ui管理器
        /// </summary>
        public static void HideAll()
        {
            foreach (var item in _uiMap)
            {
                if (item.Value && item.Value.gameObject)
                {
                    item.Value.gameObject.SetActive(false);
                }
            }
            _uiMap.Clear();

            if (_uiLoad != null)
            {
                Object.Destroy(_uiLoad);
            }
        }
    }
}
