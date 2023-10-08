using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TEngine
{
    public class PointerLongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        public float durationThreshold = 1.0f; //触发长按的时间阈值
        public float startSpeed = 3;           //开始速度
        public float addSpeed = 2;             //加速度
        public float maxValue = 25;            //最大值
        public float callIntervalTime = 0.05f; //调用间隔时间,时间为0只触发一次长按点击回调

        private bool _isPointerDown = false;
        private bool _longPressTriggered = false;
        private bool _isGreaterMaxValue = false;  //是否已经大于最大值
        private float _curTime = 0;
        private float _curCallTime = 0;

        private OnLongPressEvent _onLongPress;
        private UnityEvent _onClick;

        public UnityEvent onClick
        {
            get
            {
                if (_onClick == null)
                {
                    _onClick = new UnityEvent();
                }
                return _onClick;
            }
        }

        public OnLongPressEvent onLongPress
        {
            get
            {
                if (_onLongPress == null)
                {
                    _onLongPress = new OnLongPressEvent();
                }
                return _onLongPress;
            }
        }

        public static PointerLongPress Get(GameObject go)
        {
            PointerLongPress listener = go.GetComponent<PointerLongPress>();
            if (listener == null) listener = go.AddComponent<PointerLongPress>();
            return listener;
        }

        public void SetLongPressParam(float durationThreshold, float startSpeed, float maxValue, float addSpeed, float callIntervalTime)
        {
            this.durationThreshold = durationThreshold;
            this.startSpeed = startSpeed;
            this.maxValue = maxValue;
            this.addSpeed = addSpeed;
            this.callIntervalTime = callIntervalTime;
        }

        private void Update()
        {
            if (_isPointerDown)
            {
                if (!_longPressTriggered)
                {
                    _curTime += GameTime.deltaTime;
                    if (_curTime >= durationThreshold)
                    {
                        _longPressTriggered = true;
                        _isGreaterMaxValue = false;
                        _curTime = 0;
                        _curCallTime = 0;
                        if (callIntervalTime <= 0)
                        {
                            onLongPress?.Invoke(0);
                        }
                    }
                }
                else if (callIntervalTime > 0)
                {
                    _curTime += GameTime.deltaTime;
                    _curCallTime += GameTime.deltaTime;
                    if (_curCallTime >= callIntervalTime)
                    {
                        float value = 0;
                        if (_isGreaterMaxValue)
                        {
                            value = maxValue;
                        }
                        else
                        {
                            float curSpeed = (startSpeed + startSpeed + addSpeed * _curTime) * 0.5f;
                            value = curSpeed * _curTime;
                            if (value >= maxValue)
                            {
                                _isGreaterMaxValue = true;
                                value = maxValue;
                            }
                        }

                        //TLogger.LogInfo("value：" + value + ",长按持续时间：" + _curTime);
                        _curCallTime = 0;
                        onLongPress?.Invoke(Mathf.FloorToInt(value));
                    }
                }
            }
        }

        private void OnEnable()
        {
            _isPointerDown = false;
            _longPressTriggered = false;
            _curTime = 0;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            _curTime = 0;
            _isPointerDown = true;
            _longPressTriggered = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            _isPointerDown = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            _isPointerDown = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            if (!_longPressTriggered)
            {
                onClick.Invoke();
            }
        }
    }

    public class OnLongPressEvent : UnityEvent<int>
    {

    }
}
