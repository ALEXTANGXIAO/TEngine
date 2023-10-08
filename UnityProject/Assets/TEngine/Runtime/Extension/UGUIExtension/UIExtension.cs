using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TEngine;

public static class UIExtension
{
    public static IEnumerator FadeToAlpha(this CanvasGroup canvasGroup, float alpha, float duration,
        Action callback = null)
    {
        float time = 0f;
        float originalAlpha = canvasGroup.alpha;
        while (time < duration)
        {
            time += GameTime.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(originalAlpha, alpha, time / duration);
            yield return new WaitForEndOfFrame();
        }

        canvasGroup.alpha = alpha;

        callback?.Invoke();
    }

    public static IEnumerator SmoothValue(this Slider slider, float value, float duration, Action callback = null)
    {
        float time = 0f;
        float originalValue = slider.value;
        while (time < duration)
        {
            time += GameTime.deltaTime;
            slider.value = Mathf.Lerp(originalValue, value, time / duration);
            yield return new WaitForEndOfFrame();
        }

        slider.value = value;

        callback?.Invoke();
    }

    public static IEnumerator SmoothValue(this Scrollbar slider, float value, float duration, Action callback = null)
    {
        float time = 0f;
        float originalValue = slider.size;
        while (time < duration)
        {
            time += GameTime.deltaTime;
            slider.size = Mathf.Lerp(originalValue, value, time / duration);
            yield return new WaitForEndOfFrame();
        }

        slider.size = value;

        callback?.Invoke();
    }

    public static IEnumerator SmoothValue(this Image image, float value, float duration, Action callback = null)
    {
        float time = 0f;
        float originalValue = image.fillAmount;
        while (time < duration)
        {
            time += GameTime.deltaTime;
            image.fillAmount = Mathf.Lerp(originalValue, value, time / duration);
            yield return new WaitForEndOfFrame();
        }

        image.fillAmount = value;

        callback?.Invoke();
    }

    public static bool GetMouseDownUiPos(this UIModule uiModule, out Vector3 screenPos)
    {
        bool hadMouseDown = false;
        Vector3 mousePos = Vector3.zero;

#if UNITY_EDITOR || PLATFORM_STANDALONE_WIN
        mousePos = Input.mousePosition;
        hadMouseDown = Input.GetMouseButton(0);
#else
        if (Input.touchCount > 0)
        {
            mousePos = Input.GetTouch(0).position;
            hadMouseDown = true;
        }
        else
        {
            hadMouseDown = false;
        }
#endif

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiModule.UIRoot as RectTransform,
            mousePos,
            uiModule.UICamera, out var pos);
        screenPos = uiModule.UIRoot.TransformPoint(pos);

        return hadMouseDown;
    }
    
    /// <summary>
    /// 对字符串加自定义颜色格式
    /// </summary>
    /// <param name="desc"></param>
    /// <param name="color"></param>
    /// <returns></returns>
    public static string ToColor(this string desc, string color)
    {
        return $"<color=#{color}>{desc}</color>";
    }
}