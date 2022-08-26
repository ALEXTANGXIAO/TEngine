using UnityEngine;

namespace TEngine.Runtime.UIModule
{
    public enum TweenType
    {
        Position,
        Rotation,
        Scale,
        Alpha,
    }

    public class TweenUtil : MonoBehaviour
    {
        public bool isLocal;
        public TweenType type;
        public Vector3 from;
        public Vector3 to;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
        public float duration = 1f;
        public bool isLoop;
        public bool isPingPong;
        private float timer = 0f;
        private CanvasGroup canvasGroup;

        void Awake()
        {
            canvasGroup = gameObject.GetComponent<CanvasGroup>();
        }

        void Update()
        {
            if (duration > 0)
            {
                timer += Time.deltaTime;
                float curveValue;
                if (isLoop)
                {
                    float remainTime = timer % duration;
                    int loopCount = (int)(timer / duration);
                    float evaluateTime = remainTime / duration;
                    if (isPingPong)
                    {
                        evaluateTime = loopCount % 2 == 0 ? evaluateTime : 1 - evaluateTime;
                    }
                    curveValue = curve.Evaluate(evaluateTime);
                }
                else
                {
                    curveValue = curve.Evaluate(timer);
                }
                var lerpValue = Vector3.Lerp(from, to, curveValue);
                //if (lerpValue == lastValue)
                //{
                //    return;
                //}
                //lastValue = lerpValue;
                switch (type)
                {
                    case TweenType.Position:
                        if (isLocal)
                        {
                            transform.localPosition = lerpValue;
                        }
                        else
                        {
                            transform.position = lerpValue;
                        }
                        break;
                    case TweenType.Rotation:
                        if (isLocal)
                        {
                            transform.localEulerAngles = lerpValue;
                        }
                        else
                        {
                            transform.eulerAngles = lerpValue;
                        }
                        break;
                    case TweenType.Scale:
                        if (isLocal)
                        {
                            transform.localScale = lerpValue;
                        }
                        else
                        {
                            var value1 = VectorWiseDivision(transform.lossyScale, transform.localScale);
                            var value2 = VectorWiseDivision(lerpValue, value1);
                            transform.localScale = value2;
                        }
                        break;
                    case TweenType.Alpha:
                        if (canvasGroup != null)
                        {
                            canvasGroup.alpha = lerpValue.x;
                        }
                        else
                        {
                            TLogger.LogError("Change Alpha need Component: [CanvasGroup]");
                        }
                        break;
                }
            }
        }

        Vector3 VectorWiseDivision(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
    }
}