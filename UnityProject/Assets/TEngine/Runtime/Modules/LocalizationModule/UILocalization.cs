using TEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace TEngine
{
    public enum UIType
    {
        None = 0,
        Text = 1,
        TextMeshPro = 2,
        Image = 3,
    }

    /// <summary>
    /// UI多语言组件。
    /// </summary>
    public class UILocalization : MonoBehaviour
    {
        public UIType uiType;

        public string key = string.Empty;

        #region UI组件缓存

        private Text _text;
        private TextMeshProUGUI _textMeshProUGUI;
        private Image _image;

        #endregion

        private void Awake()
        {
            OnInitialize();
        }

        private void OnInitialize()
        {
            GameEvent.AddEventListener(LocalizationModule.OnChangedLanguage, OnHandleLocalization);
            CheckComponent();
            OnHandleLocalization();
        }

        private void CheckComponent()
        {
            switch (uiType)
            {
                case UIType.Text:
                    _text = gameObject.GetComponent<Text>();
                    break;
                case UIType.TextMeshPro:
                    _textMeshProUGUI = gameObject.GetComponent<TextMeshProUGUI>();
                    break;
                case UIType.Image:
                    _image = gameObject.GetComponent<Image>();
                    break;
            }
        }

        private void OnHandleLocalization()
        {
            if (gameObject == null || string.IsNullOrEmpty(key))
            {
                return;
            }

            string result = GameModule.Localization.GetString(key);
            switch (uiType)
            {
                case UIType.Text:
                    _text.text = result;
                    break;
                case UIType.TextMeshPro:
                    _textMeshProUGUI.text = result;
                    break;
                case UIType.Image:
                    _image.SetSprite(result);
                    break;
            }
        }

        private void OnDestroy()
        {
            GameEvent.RemoveEventListener(LocalizationModule.OnChangedLanguage, OnHandleLocalization);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UILocalization))]
public class UILocalizationInspector : Editor
{
    private bool _check = false;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        if (!_check)
        {
            UILocalization t = (UILocalization)target;
            if (t.gameObject.GetComponent<Text>() != null)
            {
                t.uiType = UIType.Text;
            }
            else if (t.gameObject.GetComponent<TextMeshProUGUI>() != null)
            {
                t.uiType = UIType.TextMeshPro;
            }
            else if (t.gameObject.GetComponent<Image>() != null)
            {
                t.uiType = UIType.Image;
            }

            _check = true;
        }

        serializedObject.ApplyModifiedProperties();
        base.OnInspectorGUI();
    }
}
#endif