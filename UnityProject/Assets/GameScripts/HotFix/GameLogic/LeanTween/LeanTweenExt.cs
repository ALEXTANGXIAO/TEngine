using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace GameLogic
{


    public static class LeanTweenExt
    {
        //LeanTween.addListener
        //LeanTween.alpha
        public static LTDescr LeanAlpha(this GameObject gameObject, float to, float time)
        {
            return LeanTween.alpha(gameObject, to, time);
        }

        //LeanTween.alphaCanvas
        public static LTDescr LeanAlphaVertex(this GameObject gameObject, float to, float time)
        {
            return LeanTween.alphaVertex(gameObject, to, time);
        }

        //LeanTween.alpha (RectTransform)
        public static LTDescr LeanAlpha(this RectTransform rectTransform, float to, float time)
        {
            return LeanTween.alpha(rectTransform, to, time);
        }

        //LeanTween.alphaCanvas
        public static LTDescr LeanAlpha(this CanvasGroup canvas, float to, float time)
        {
            return LeanTween.alphaCanvas(canvas, to, time);
        }

        //LeanTween.alphaText
        public static LTDescr LeanAlphaText(this RectTransform rectTransform, float to, float time)
        {
            return LeanTween.alphaText(rectTransform, to, time);
        }

        //LeanTween.cancel
        public static void LeanCancel(this GameObject gameObject)
        {
            LeanTween.cancel(gameObject);
        }

        public static void LeanCancel(this GameObject gameObject, bool callOnComplete)
        {
            LeanTween.cancel(gameObject, callOnComplete);
        }

        public static void LeanCancel(this GameObject gameObject, int uniqueId, bool callOnComplete = false)
        {
            LeanTween.cancel(gameObject, uniqueId, callOnComplete);
        }

        //LeanTween.cancel
        public static void LeanCancel(this RectTransform rectTransform)
        {
            LeanTween.cancel(rectTransform);
        }

        //LeanTween.cancelAll
        //LeanTween.color
        public static LTDescr LeanColor(this GameObject gameObject, Color to, float time)
        {
            return LeanTween.color(gameObject, to, time);
        }

        //LeanTween.colorText
        public static LTDescr LeanColorText(this RectTransform rectTransform, Color to, float time)
        {
            return LeanTween.colorText(rectTransform, to, time);
        }

        //LeanTween.delayedCall
        public static LTDescr LeanDelayedCall(this GameObject gameObject, float delayTime, System.Action callback)
        {
            return LeanTween.delayedCall(gameObject, delayTime, callback);
        }

        public static LTDescr LeanDelayedCall(this GameObject gameObject, float delayTime,
            System.Action<object> callback)
        {
            return LeanTween.delayedCall(gameObject, delayTime, callback);
        }

        //LeanTween.isPaused
        public static bool LeanIsPaused(this GameObject gameObject)
        {
            return LeanTween.isPaused(gameObject);
        }

        public static bool LeanIsPaused(this RectTransform rectTransform)
        {
            return LeanTween.isPaused(rectTransform);
        }

        //LeanTween.isTweening
        public static bool LeanIsTweening(this GameObject gameObject)
        {
            return LeanTween.isTweening(gameObject);
        }

        //LeanTween.isTweening
        //LeanTween.move
        public static LTDescr LeanMove(this GameObject gameObject, Vector3 to, float time)
        {
            return LeanTween.move(gameObject, to, time);
        }

        public static LTDescr LeanMove(this Transform transform, Vector3 to, float time)
        {
            return LeanTween.move(transform.gameObject, to, time);
        }

        public static LTDescr LeanMove(this RectTransform rectTransform, Vector3 to, float time)
        {
            return LeanTween.move(rectTransform, to, time);
        }

        //LeanTween.move
        public static LTDescr LeanMove(this GameObject gameObject, Vector2 to, float time)
        {
            return LeanTween.move(gameObject, to, time);
        }

        public static LTDescr LeanMove(this Transform transform, Vector2 to, float time)
        {
            return LeanTween.move(transform.gameObject, to, time);
        }

        //LeanTween.move
        public static LTDescr LeanMove(this GameObject gameObject, Vector3[] to, float time)
        {
            return LeanTween.move(gameObject, to, time);
        }

        public static LTDescr LeanMove(this GameObject gameObject, LTBezierPath to, float time)
        {
            return LeanTween.move(gameObject, to, time);
        }

        public static LTDescr LeanMove(this GameObject gameObject, LTSpline to, float time)
        {
            return LeanTween.move(gameObject, to, time);
        }

        public static LTDescr LeanMove(this Transform transform, Vector3[] to, float time)
        {
            return LeanTween.move(transform.gameObject, to, time);
        }

        public static LTDescr LeanMove(this Transform transform, LTBezierPath to, float time)
        {
            return LeanTween.move(transform.gameObject, to, time);
        }

        public static LTDescr LeanMove(this Transform transform, LTSpline to, float time)
        {
            return LeanTween.move(transform.gameObject, to, time);
        }

        //LeanTween.moveLocal
        public static LTDescr LeanMoveLocal(this GameObject gameObject, Vector3 to, float time)
        {
            return LeanTween.moveLocal(gameObject, to, time);
        }

        public static LTDescr LeanMoveLocal(this GameObject gameObject, LTBezierPath to, float time)
        {
            return LeanTween.moveLocal(gameObject, to, time);
        }

        public static LTDescr LeanMoveLocal(this GameObject gameObject, LTSpline to, float time)
        {
            return LeanTween.moveLocal(gameObject, to, time);
        }

        public static LTDescr LeanMoveLocal(this Transform transform, Vector3 to, float time)
        {
            return LeanTween.moveLocal(transform.gameObject, to, time);
        }

        public static LTDescr LeanMoveLocal(this Transform transform, LTBezierPath to, float time)
        {
            return LeanTween.moveLocal(transform.gameObject, to, time);
        }

        public static LTDescr LeanMoveLocal(this Transform transform, LTSpline to, float time)
        {
            return LeanTween.moveLocal(transform.gameObject, to, time);
        }

        //LeanTween.moveLocal
        public static LTDescr LeanMoveLocalX(this GameObject gameObject, float to, float time)
        {
            return LeanTween.moveLocalX(gameObject, to, time);
        }

        public static LTDescr LeanMoveLocalY(this GameObject gameObject, float to, float time)
        {
            return LeanTween.moveLocalY(gameObject, to, time);
        }

        public static LTDescr LeanMoveLocalZ(this GameObject gameObject, float to, float time)
        {
            return LeanTween.moveLocalZ(gameObject, to, time);
        }

        public static LTDescr LeanMoveLocalX(this Transform transform, float to, float time)
        {
            return LeanTween.moveLocalX(transform.gameObject, to, time);
        }

        public static LTDescr LeanMoveLocalY(this Transform transform, float to, float time)
        {
            return LeanTween.moveLocalY(transform.gameObject, to, time);
        }

        public static LTDescr LeanMoveLocalZ(this Transform transform, float to, float time)
        {
            return LeanTween.moveLocalZ(transform.gameObject, to, time);
        }

        //LeanTween.moveSpline
        public static LTDescr LeanMoveSpline(this GameObject gameObject, Vector3[] to, float time)
        {
            return LeanTween.moveSpline(gameObject, to, time);
        }

        public static LTDescr LeanMoveSpline(this GameObject gameObject, LTSpline to, float time)
        {
            return LeanTween.moveSpline(gameObject, to, time);
        }

        public static LTDescr LeanMoveSpline(this Transform transform, Vector3[] to, float time)
        {
            return LeanTween.moveSpline(transform.gameObject, to, time);
        }

        public static LTDescr LeanMoveSpline(this Transform transform, LTSpline to, float time)
        {
            return LeanTween.moveSpline(transform.gameObject, to, time);
        }

        //LeanTween.moveSplineLocal
        public static LTDescr LeanMoveSplineLocal(this GameObject gameObject, Vector3[] to, float time)
        {
            return LeanTween.moveSplineLocal(gameObject, to, time);
        }

        public static LTDescr LeanMoveSplineLocal(this Transform transform, Vector3[] to, float time)
        {
            return LeanTween.moveSplineLocal(transform.gameObject, to, time);
        }

        //LeanTween.moveX
        public static LTDescr LeanMoveX(this GameObject gameObject, float to, float time)
        {
            return LeanTween.moveX(gameObject, to, time);
        }

        public static LTDescr LeanMoveX(this Transform transform, float to, float time)
        {
            return LeanTween.moveX(transform.gameObject, to, time);
        }

        //LeanTween.moveX (RectTransform)
        public static LTDescr LeanMoveX(this RectTransform rectTransform, float to, float time)
        {
            return LeanTween.moveX(rectTransform, to, time);
        }

        //LeanTween.moveY
        public static LTDescr LeanMoveY(this GameObject gameObject, float to, float time)
        {
            return LeanTween.moveY(gameObject, to, time);
        }

        public static LTDescr LeanMoveY(this Transform transform, float to, float time)
        {
            return LeanTween.moveY(transform.gameObject, to, time);
        }

        //LeanTween.moveY (RectTransform)
        public static LTDescr LeanMoveY(this RectTransform rectTransform, float to, float time)
        {
            return LeanTween.moveY(rectTransform, to, time);
        }

        //LeanTween.moveZ
        public static LTDescr LeanMoveZ(this GameObject gameObject, float to, float time)
        {
            return LeanTween.moveZ(gameObject, to, time);
        }

        public static LTDescr LeanMoveZ(this Transform transform, float to, float time)
        {
            return LeanTween.moveZ(transform.gameObject, to, time);
        }

        //LeanTween.moveZ (RectTransform)
        public static LTDescr LeanMoveZ(this RectTransform rectTransform, float to, float time)
        {
            return LeanTween.moveZ(rectTransform, to, time);
        }

        //LeanTween.pause
        public static void LeanPause(this GameObject gameObject)
        {
            LeanTween.pause(gameObject);
        }

        //LeanTween.play
        public static LTDescr LeanPlay(this RectTransform rectTransform, UnityEngine.Sprite[] sprites)
        {
            return LeanTween.play(rectTransform, sprites);
        }

        //LeanTween.removeListener
        //LeanTween.resume
        public static void LeanResume(this GameObject gameObject)
        {
            LeanTween.resume(gameObject);
        }

        //LeanTween.resumeAll
        //LeanTween.rotate 
        public static LTDescr LeanRotate(this GameObject gameObject, Vector3 to, float time)
        {
            return LeanTween.rotate(gameObject, to, time);
        }

        public static LTDescr LeanRotate(this Transform transform, Vector3 to, float time)
        {
            return LeanTween.rotate(transform.gameObject, to, time);
        }

        //LeanTween.rotate
        //LeanTween.rotate (RectTransform)
        public static LTDescr LeanRotate(this RectTransform rectTransform, Vector3 to, float time)
        {
            return LeanTween.rotate(rectTransform, to, time);
        }

        //LeanTween.rotateAround
        public static LTDescr LeanRotateAround(this GameObject gameObject, Vector3 axis, float add, float time)
        {
            return LeanTween.rotateAround(gameObject, axis, add, time);
        }

        public static LTDescr LeanRotateAround(this Transform transform, Vector3 axis, float add, float time)
        {
            return LeanTween.rotateAround(transform.gameObject, axis, add, time);
        }

        //LeanTween.rotateAround (RectTransform)
        public static LTDescr LeanRotateAround(this RectTransform rectTransform, Vector3 axis, float add, float time)
        {
            return LeanTween.rotateAround(rectTransform, axis, add, time);
        }

        //LeanTween.rotateAroundLocal
        public static LTDescr LeanRotateAroundLocal(this GameObject gameObject, Vector3 axis, float add, float time)
        {
            return LeanTween.rotateAroundLocal(gameObject, axis, add, time);
        }

        public static LTDescr LeanRotateAroundLocal(this Transform transform, Vector3 axis, float add, float time)
        {
            return LeanTween.rotateAroundLocal(transform.gameObject, axis, add, time);
        }

        //LeanTween.rotateAround (RectTransform)
        public static LTDescr LeanRotateAroundLocal(this RectTransform rectTransform, Vector3 axis, float add,
            float time)
        {
            return LeanTween.rotateAroundLocal(rectTransform, axis, add, time);
        }

        //LeanTween.rotateLocal
        //LeanTween.rotateX
        public static LTDescr LeanRotateX(this GameObject gameObject, float to, float time)
        {
            return LeanTween.rotateX(gameObject, to, time);
        }

        public static LTDescr LeanRotateX(this Transform transform, float to, float time)
        {
            return LeanTween.rotateX(transform.gameObject, to, time);
        }

        //LeanTween.rotateY
        public static LTDescr LeanRotateY(this GameObject gameObject, float to, float time)
        {
            return LeanTween.rotateY(gameObject, to, time);
        }

        public static LTDescr LeanRotateY(this Transform transform, float to, float time)
        {
            return LeanTween.rotateY(transform.gameObject, to, time);
        }

        //LeanTween.rotateZ
        public static LTDescr LeanRotateZ(this GameObject gameObject, float to, float time)
        {
            return LeanTween.rotateZ(gameObject, to, time);
        }

        public static LTDescr LeanRotateZ(this Transform transform, float to, float time)
        {
            return LeanTween.rotateZ(transform.gameObject, to, time);
        }

        //LeanTween.scale
        public static LTDescr LeanScale(this GameObject gameObject, Vector3 to, float time)
        {
            return LeanTween.scale(gameObject, to, time);
        }

        public static LTDescr LeanScale(this Transform transform, Vector3 to, float time)
        {
            return LeanTween.scale(transform.gameObject, to, time);
        }

        //LeanTween.scale (GUI)
        //LeanTween.scale (RectTransform)
        public static LTDescr LeanScale(this RectTransform rectTransform, Vector3 to, float time)
        {
            return LeanTween.scale(rectTransform, to, time);
        }

        //LeanTween.scaleX
        public static LTDescr LeanScaleX(this GameObject gameObject, float to, float time)
        {
            return LeanTween.scaleX(gameObject, to, time);
        }

        public static LTDescr LeanScaleX(this Transform transform, float to, float time)
        {
            return LeanTween.scaleX(transform.gameObject, to, time);
        }

        //LeanTween.scaleY
        public static LTDescr LeanScaleY(this GameObject gameObject, float to, float time)
        {
            return LeanTween.scaleY(gameObject, to, time);
        }

        public static LTDescr LeanScaleY(this Transform transform, float to, float time)
        {
            return LeanTween.scaleY(transform.gameObject, to, time);
        }

        //LeanTween.scaleZ
        public static LTDescr LeanScaleZ(this GameObject gameObject, float to, float time)
        {
            return LeanTween.scaleZ(gameObject, to, time);
        }

        public static LTDescr LeanScaleZ(this Transform transform, float to, float time)
        {
            return LeanTween.scaleZ(transform.gameObject, to, time);
        }

        //LeanTween.sequence
        //LeanTween.size (RectTransform)
        public static LTDescr LeanSize(this RectTransform rectTransform, Vector2 to, float time)
        {
            return LeanTween.size(rectTransform, to, time);
        }

        //LeanTween.tweensRunning
        //LeanTween.value (Color)
        public static LTDescr LeanValue(this GameObject gameObject, Color from, Color to, float time)
        {
            return LeanTween.value(gameObject, from, to, time);
        }

        //LeanTween.value (Color)
        //LeanTween.value (float)
        public static LTDescr LeanValue(this GameObject gameObject, float from, float to, float time)
        {
            return LeanTween.value(gameObject, from, to, time);
        }

        public static LTDescr LeanValue(this GameObject gameObject, Vector2 from, Vector2 to, float time)
        {
            return LeanTween.value(gameObject, from, to, time);
        }

        public static LTDescr LeanValue(this GameObject gameObject, Vector3 from, Vector3 to, float time)
        {
            return LeanTween.value(gameObject, from, to, time);
        }

        //LeanTween.value (float)
        public static LTDescr LeanValue(this GameObject gameObject, Action<float> callOnUpdate, float from, float to,
            float time)
        {
            return LeanTween.value(gameObject, callOnUpdate, from, to, time);
        }

        public static LTDescr LeanValue(this GameObject gameObject, Action<float, float> callOnUpdate, float from,
            float to, float time)
        {
            return LeanTween.value(gameObject, callOnUpdate, from, to, time);
        }

        public static LTDescr LeanValue(this GameObject gameObject, Action<float, object> callOnUpdate, float from,
            float to, float time)
        {
            return LeanTween.value(gameObject, callOnUpdate, from, to, time);
        }

        public static LTDescr LeanValue(this GameObject gameObject, Action<Color> callOnUpdate, Color from, Color to,
            float time)
        {
            return LeanTween.value(gameObject, callOnUpdate, from, to, time);
        }

        public static LTDescr LeanValue(this GameObject gameObject, Action<Vector2> callOnUpdate, Vector2 from,
            Vector2 to, float time)
        {
            return LeanTween.value(gameObject, callOnUpdate, from, to, time);
        }

        public static LTDescr LeanValue(this GameObject gameObject, Action<Vector3> callOnUpdate, Vector3 from,
            Vector3 to, float time)
        {
            return LeanTween.value(gameObject, callOnUpdate, from, to, time);
        }

        //LeanTween.value (float)
        //LeanTween.value (float, object)
        //LeanTween.value (Vector2)
        //LeanTween.value (Vector2)
        //LeanTween.value (Vector3)
        //LeanTween.value (Vector3)
    }

}