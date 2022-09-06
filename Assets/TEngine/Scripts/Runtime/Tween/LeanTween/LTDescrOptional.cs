using UnityEngine;
using System;

namespace TEngine.Runtime
{
    public class LTDescrOptional
    {
        public Transform toTrans { get; set; }
        public Vector3 point { get; set; }
        public Vector3 axis { get; set; }
        public float lastVal { get; set; }
        public Quaternion origRotation { get; set; }
        public LTBezierPath path { get; set; }
        public LTSpline spline { get; set; }
        public AnimationCurve animationCurve;
        public int initFrameCount;

        public LTRect ltRect { get; set; } // maybe get rid of this eventually

        public Action<float> onUpdateFloat { get; set; }
        public Action<float, float> onUpdateFloatRatio { get; set; }
        public Action<float, object> onUpdateFloatObject { get; set; }
        public Action<Vector2> onUpdateVector2 { get; set; }
        public Action<Vector3> onUpdateVector3 { get; set; }
        public Action<Vector3, object> onUpdateVector3Object { get; set; }
        public Action<Color> onUpdateColor { get; set; }
        public Action<Color, object> onUpdateColorObject { get; set; }
        public Action onComplete { get; set; }
        public Action<object> onCompleteObject { get; set; }
        public object onCompleteParam { get; set; }
        public object onUpdateParam { get; set; }
        public Action onStart { get; set; }

        public void reset()
        {
            animationCurve = null;

            this.onUpdateFloat = null;
            this.onUpdateFloatRatio = null;
            this.onUpdateVector2 = null;
            this.onUpdateVector3 = null;
            this.onUpdateFloatObject = null;
            this.onUpdateVector3Object = null;
            this.onUpdateColor = null;
            this.onComplete = null;
            this.onCompleteObject = null;
            this.onCompleteParam = null;
            this.onStart = null;

            this.point = Vector3.zero;
            this.initFrameCount = 0;
        }

        public void callOnUpdate(float val, float ratioPassed)
        {
            if (this.onUpdateFloat != null)
                this.onUpdateFloat(val);

            if (this.onUpdateFloatRatio != null)
            {
                this.onUpdateFloatRatio(val, ratioPassed);
            }
            else if (this.onUpdateFloatObject != null)
            {
                this.onUpdateFloatObject(val, this.onUpdateParam);
            }
            else if (this.onUpdateVector3Object != null)
            {
                this.onUpdateVector3Object(LTDescr.newVect, this.onUpdateParam);
            }
            else if (this.onUpdateVector3 != null)
            {
                this.onUpdateVector3(LTDescr.newVect);
            }
            else if (this.onUpdateVector2 != null)
            {
                this.onUpdateVector2(new Vector2(LTDescr.newVect.x, LTDescr.newVect.y));
            }
        }
    }
}