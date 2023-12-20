using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace TEngine
{
    [Serializable]
    public class LocalizationInfo : IDisposable
    {
        public Graphic component;
        public UIType uiType;
        public string key = string.Empty;

        public void Dispose()
        {
            component = null;
            uiType = UIType.None;
            key = string.Empty;
        }
    }

    public class UILocalizationGroup : MonoBehaviour
    {
        [InlineButton("R")] public List<LocalizationInfo> localizationGroup = new List<LocalizationInfo>();

        private void Awake()
        {
            OnInitialize();
        }

        private void OnInitialize()
        {
            GameEvent.AddEventListener(LocalizationModule.OnChangedLanguage, OnHandleLocalization);
            OnHandleLocalization();
        }

        private void OnHandleLocalization()
        {
            foreach (var localizationInfo in localizationGroup)
            {
                if (localizationInfo == null || localizationInfo.key == null || localizationInfo.component == null)
                {
                    continue;
                }

                string result = GameModule.Localization.GetString(localizationInfo.key);
                switch (localizationInfo.uiType)
                {
                    case UIType.Text:
                        ((Text)localizationInfo.component).text = result;
                        break;
                    case UIType.TextMeshPro:
                        ((TextMeshProUGUI)localizationInfo.component).text = result;
                        break;
                    case UIType.Image:
                        ((Image)localizationInfo.component).SetSprite(result);
                        break;
                }
            }
        }

        private void OnDestroy()
        {
            GameEvent.RemoveEventListener(LocalizationModule.OnChangedLanguage, OnHandleLocalization);
            var count = localizationGroup.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var localizationInfo = localizationGroup[i];
                localizationInfo.Dispose();
                localizationInfo = null;
            }

            localizationGroup.Clear();
        }

        private void R()
        {
            var count = localizationGroup.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var localizationInfo = localizationGroup[i];
                if (localizationInfo == null)
                {
                    localizationGroup.RemoveAt(i);
                    continue;
                }

                if (IsComponentHadMissing(localizationInfo.component))
                {
                    localizationGroup.RemoveAt(i);
                    localizationInfo.Dispose();
                    localizationInfo = null;
                    continue;
                }
            }

            var texts = gameObject.transform.GetComponentsInChildren<Text>();
            var textMeshPros = gameObject.transform.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (localizationGroup.Find(t => t.component == text) != null)
                {
                    continue;
                }

                LocalizationInfo localizationInfo = new LocalizationInfo();
                localizationInfo.component = text;
                localizationInfo.uiType = UIType.Text;
                localizationGroup.Add(localizationInfo);
            }

            foreach (var textMeshProUGUI in textMeshPros)
            {
                if (localizationGroup.Find(t => t.component == textMeshProUGUI) != null)
                {
                    continue;
                }

                LocalizationInfo localizationInfo = new LocalizationInfo();
                localizationInfo.component = textMeshProUGUI;
                localizationInfo.uiType = UIType.TextMeshPro;
                localizationGroup.Add(localizationInfo);
            }
        }

        private bool IsComponentHadMissing(Object component)
        {
            return component == null;
        }
    }
}