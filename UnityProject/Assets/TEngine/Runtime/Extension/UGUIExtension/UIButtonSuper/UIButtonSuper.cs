using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using TEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;

[CustomEditor(typeof(UIButtonSuper), true)]
[CanEditMultipleObjects]
public class UIButtonSuperEditor : ButtonEditor
{
    private SerializedProperty m_ButtonUISounds;
    private SerializedProperty m_CanClick;
    private SerializedProperty m_CanDoubleClick;
    private SerializedProperty m_DoubleClickIntervalTime;
    private SerializedProperty onDoubleClick;

    private SerializedProperty m_CanLongPress;
    private SerializedProperty m_ResponseOnceByPress;
    private SerializedProperty m_LongPressDurationTime;
    private SerializedProperty onPress;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_ButtonUISounds = serializedObject.FindProperty("m_ButtonUISounds");
        m_CanClick = serializedObject.FindProperty("m_CanClick");
        m_CanDoubleClick = serializedObject.FindProperty("m_CanDoubleClick");
        m_DoubleClickIntervalTime = serializedObject.FindProperty("m_DoubleClickIntervalTime");
        onDoubleClick = serializedObject.FindProperty("onDoubleClick");

        m_CanLongPress = serializedObject.FindProperty("m_CanLongPress");
        m_ResponseOnceByPress = serializedObject.FindProperty("m_ResponseOnceByPress");
        m_LongPressDurationTime = serializedObject.FindProperty("m_LongPressDurationTime");
        onPress = serializedObject.FindProperty("onPress");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        EditorGUILayout.PropertyField(m_ButtonUISounds); //显示我们创建的属性
        EditorGUILayout.PropertyField(m_CanClick); //显示我们创建的属性
        EditorGUILayout.Space(); //空行
        EditorGUILayout.PropertyField(m_CanDoubleClick); //显示我们创建的属性
        EditorGUILayout.PropertyField(m_DoubleClickIntervalTime); //显示我们创建的属性
        EditorGUILayout.PropertyField(onDoubleClick); //显示我们创建的属性
        EditorGUILayout.Space(); //空行
        EditorGUILayout.PropertyField(m_CanLongPress); //显示我们创建的属性
        EditorGUILayout.PropertyField(m_ResponseOnceByPress); //显示我们创建的属性
        EditorGUILayout.PropertyField(m_LongPressDurationTime); //显示我们创建的属性
        EditorGUILayout.PropertyField(onPress); //显示我们创建的属性
        serializedObject.ApplyModifiedProperties();
    }
}

#endif

public enum ButtonSoundType
{
    Down,
    Up,
    Click,
    Enter,
    Exit,
    Drag
}

[Serializable]
public class ButtonSoundCell
{
    public ButtonSoundType ButtonSoundType = ButtonSoundType.Click;
    public string ButtonUISoundName = "ui_click_button";
}

public delegate void ButtonBeginDragCallback(PointerEventData eventData);

public delegate void ButtonDragCallback(PointerEventData eventData);

public delegate void ButtonEndDragCallback(PointerEventData eventData);

public class UIButtonSuper : Button, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public List<ButtonSoundCell> m_ButtonUISounds = new List<ButtonSoundCell>() { new ButtonSoundCell() };
    [Tooltip("是否可以点击")] public bool m_CanClick = true;
    [Tooltip("是否可以双击")] public bool m_CanDoubleClick = false;
    [Tooltip("双击间隔时长")] public float m_DoubleClickIntervalTime = 0.1f;
    [Tooltip("双击事件")] public ButtonClickedEvent onDoubleClick;
    [Tooltip("是否可以长按")] public bool m_CanLongPress = false;
    [Tooltip("长按是否只响应一次")] public bool m_ResponseOnceByPress = false;
    [Tooltip("长按满足间隔")] public float m_LongPressDurationTime = 1;

    [Tooltip("长按事件")] public ButtonClickedEvent onPress;

    //public ButtonClickedEvent onClick;
    public ButtonBeginDragCallback onBeginDrag;
    public ButtonDragCallback onDrag;
    public ButtonEndDragCallback onEndDrag;

    private bool isDown = false;
    private bool isPress = false;
    private bool isDownExit = false;
    private float downTime = 0;

    private int fingerId = int.MinValue;

    public bool IsDraging
    {
        get { return fingerId != int.MinValue; }
    } //摇杆拖拽状态

    public int FingerId
    {
        get { return fingerId; }
    }

    private float clickIntervalTime = 0;
    private int clickTimes = 0;

    void Update()
    {
        if (isDown)
        {
            if (!m_CanLongPress)
            {
                return;
            }

            if (m_ResponseOnceByPress && isPress)
            {
                return;
            }

            downTime += GameTime.deltaTime;
            if (downTime > m_LongPressDurationTime)
            {
                isPress = true;
                onPress.Invoke();
            }
        }

        if (clickTimes >= 1)
        {
            if (!m_CanLongPress && !m_CanDoubleClick && m_CanClick)
            {
                onClick.Invoke();
                clickTimes = 0;
            }
            else
            {
                clickIntervalTime += GameTime.deltaTime;
                if (clickIntervalTime >= m_DoubleClickIntervalTime)
                {
                    if (clickTimes >= 2)
                    {
                        if (m_CanDoubleClick)
                        {
                            onDoubleClick.Invoke();
                        }
                    }
                    else
                    {
                        if (m_CanClick)
                        {
                            onClick.Invoke();
                        }
                    }

                    clickTimes = 0;
                    clickIntervalTime = 0;
                }
            }
        }
    }

    /// <summary>
    /// 是否按钮按下
    /// </summary>
    public bool IsDown
    {
        get { return isDown; }
    }

    /// <summary>
    /// 是否按钮长按
    /// </summary>
    public bool IsPress
    {
        get { return isPress; }
    }

    /// <summary>
    /// 是否按钮按下后离开按钮位置
    /// </summary>
    public bool IsDownExit
    {
        get { return isDownExit; }
    }

    public ButtonSoundCell GetButtonSound(ButtonSoundType buttonSoundType)
    {
        foreach (var buttonSound in m_ButtonUISounds)
        {
            if (buttonSound.ButtonSoundType == buttonSoundType)
            {
                return buttonSound;
            }
        }

        return null;
    }

    private void PlayButtonSound(ButtonSoundType buttonSoundType)
    {
        ButtonSoundCell buttonSound = GetButtonSound(buttonSoundType);
        if (buttonSound == null)
        {
            return;
        }

        GameModule.Audio.Play(TEngine.AudioType.Sound, buttonSound.ButtonUISoundName);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        PlayButtonSound(ButtonSoundType.Enter);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (eventData.pointerId < -1 || IsDraging) return; //适配 Touch：只响应一个Touch；适配鼠标：只响应左键
        fingerId = eventData.pointerId;
        isDown = true;
        isDownExit = false;
        downTime = 0;
        PlayButtonSound(ButtonSoundType.Down);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (fingerId != eventData.pointerId) return; //正确的手指抬起时才会；
        fingerId = int.MinValue;
        isDown = false;
        isDownExit = true;
        PlayButtonSound(ButtonSoundType.Up);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (fingerId != eventData.pointerId) return; //正确的手指抬起时才会；
        isPress = false;
        isDownExit = true;
        PlayButtonSound(ButtonSoundType.Exit);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (!isPress)
        {
            clickTimes += 1;
        }
        else
            isPress = false;

        PlayButtonSound(ButtonSoundType.Click);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        onBeginDrag?.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        PlayButtonSound(ButtonSoundType.Drag);
        onDrag?.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        onEndDrag?.Invoke(eventData);
    }
}