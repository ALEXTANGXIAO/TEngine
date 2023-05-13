using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Internal;
using Object = UnityEngine.Object;

namespace TEngine
{
    public static partial class Utility
    {
        /// <summary>
        /// Unity相关的实用函数。
        /// </summary>
        public static partial class Unity
        {
            private static GameObject _entity;
            private static MainBehaviour _behaviour;
            public static MainBehaviour Behaviour => _behaviour;

            #region 控制协程Coroutine

            public static Coroutine StartCoroutine(string methodName)
            {
                if (string.IsNullOrEmpty(methodName))
                {
                    return null;
                }

                _MakeEntity();
                return _behaviour.StartCoroutine(methodName);
            }

            public static Coroutine StartCoroutine(IEnumerator routine)
            {
                if (routine == null)
                {
                    return null;
                }

                _MakeEntity();
                return _behaviour.StartCoroutine(routine);
            }

            public static Coroutine StartCoroutine(string methodName, [DefaultValue("null")] object value)
            {
                if (string.IsNullOrEmpty(methodName))
                {
                    return null;
                }

                _MakeEntity();
                return _behaviour.StartCoroutine(methodName, value);
            }

            public static void StopCoroutine(string methodName)
            {
                if (string.IsNullOrEmpty(methodName))
                {
                    return;
                }

                if (_entity != null)
                {
                    _behaviour.StopCoroutine(methodName);
                }
            }

            public static void StopCoroutine(IEnumerator routine)
            {
                if (routine == null)
                {
                    return;
                }

                if (_entity != null)
                {
                    _behaviour.StopCoroutine(routine);
                }
            }

            public static void StopCoroutine(Coroutine routine)
            {
                if (routine == null)
                    return;

                if (_entity != null)
                {
                    _behaviour.StopCoroutine(routine);
                    routine = null;
                }
            }

            public static void StopAllCoroutines()
            {
                if (_entity != null)
                {
                    _behaviour.StopAllCoroutines();
                }
            }

            #endregion

            #region 注入UnityUpdate/FixedUpdate/LateUpdate

            /// <summary>
            /// 为给外部提供的 添加帧更新事件。
            /// </summary>
            /// <param name="fun"></param>
            public static void AddUpdateListener(UnityAction fun)
            {
                _MakeEntity();
                _behaviour.AddUpdateListener(fun);
            }

            /// <summary>
            /// 为给外部提供的 添加物理帧更新事件。
            /// </summary>
            /// <param name="fun"></param>
            public static void AddFixedUpdateListener(UnityAction fun)
            {
                _MakeEntity();
                _behaviour.AddFixedUpdateListener(fun);
            }

            /// <summary>
            /// 为给外部提供的 添加Late帧更新事件。
            /// </summary>
            /// <param name="fun"></param>
            public static void AddLateUpdateListener(UnityAction fun)
            {
                _MakeEntity();
                _behaviour.AddLateUpdateListener(fun);
            }

            /// <summary>
            /// 移除帧更新事件。
            /// </summary>
            /// <param name="fun"></param>
            public static void RemoveUpdateListener(UnityAction fun)
            {
                _MakeEntity();
                _behaviour.RemoveUpdateListener(fun);
            }

            /// <summary>
            /// 移除物理帧更新事件。
            /// </summary>
            /// <param name="fun"></param>
            public static void RemoveFixedUpdateListener(UnityAction fun)
            {
                _MakeEntity();
                _behaviour.RemoveFixedUpdateListener(fun);
            }

            /// <summary>
            /// 移除Late帧更新事件。
            /// </summary>
            /// <param name="fun"></param>
            public static void RemoveLateUpdateListener(UnityAction fun)
            {
                _MakeEntity();
                _behaviour.RemoveLateUpdateListener(fun);
            }

            #endregion

            #region Unity Events 注入
            /// <summary>
            /// 为给外部提供的Destroy注册事件。
            /// </summary>
            /// <param name="fun"></param>
            public static void AddDestroyListener(UnityAction fun)
            {
                _MakeEntity();
                _behaviour.AddDestroyListener(fun);
            }

            /// <summary>
            /// 为给外部提供的Destroy反注册事件。
            /// </summary>
            /// <param name="fun"></param>
            public static void RemoveDestroyListener(UnityAction fun)
            {
                _MakeEntity();
                _behaviour.RemoveDestroyListener(fun);
            }
                
            /// <summary>
            /// 为给外部提供的OnDrawGizmos注册事件。
            /// </summary>
            /// <param name="fun"></param>
            public static void AddOnDrawGizmosListener(UnityAction fun)
            {
                _MakeEntity();
                _behaviour.RemoveDestroyListener(fun);
            }

            /// <summary>
            /// 为给外部提供的OnDrawGizmos反注册事件。
            /// </summary>
            /// <param name="fun"></param>
            public static void RemoveOnDrawGizmosListener(UnityAction fun)
            {
                _MakeEntity();
                _behaviour.RemoveDestroyListener(fun);
            }
                
            /// <summary>
            /// 为给外部提供的OnApplicationPause注册事件。
            /// </summary>
            /// <param name="fun"></param>
            public static void AddOnApplicationPauseListener(UnityAction<bool> fun)
            {
                _MakeEntity();
                _behaviour.AddOnApplicationPauseListener(fun);
            }

            /// <summary>
            /// 为给外部提供的OnApplicationPause反注册事件。
            /// </summary>
            /// <param name="fun"></param>
            public static void RemoveOnApplicationPauseListener(UnityAction<bool> fun)
            {
                _MakeEntity();
                _behaviour.AddOnApplicationPauseListener(fun);
            }
            #endregion
            
            /// <summary>
            /// 释放Behaviour生命周期。
            /// </summary>
            public static void Release()
            {
                _MakeEntity();
                _behaviour.Release();
            }

            private static void _MakeEntity()
            {
                if (_entity != null)
                {
                    return;
                }

                _entity = new GameObject("__MonoUtility__")
                {
                    hideFlags = HideFlags.HideAndDontSave
                };
                _entity.SetActive(true);

#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                {
                    Object.DontDestroyOnLoad(_entity);
                }

                UnityEngine.Assertions.Assert.IsFalse(_behaviour);
                _behaviour = _entity.AddComponent<MainBehaviour>();
            }

            public class MainBehaviour : MonoBehaviour
            {
                private event UnityAction updateEvent;
                private event UnityAction fixedUpdateEvent;
                private event UnityAction lateUpdateEvent;
                private event UnityAction destroyEvent;
                private event UnityAction onDrawGizmosEvent; 
                private event UnityAction<bool> onApplicationPause;

                void Update()
                {
                    if (updateEvent != null)
                    {
                        updateEvent();
                    }
                }

                void FixedUpdate()
                {
                    if (fixedUpdateEvent != null)
                    {
                        fixedUpdateEvent();
                    }
                }

                void LateUpdate()
                {
                    if (lateUpdateEvent != null)
                    {
                        lateUpdateEvent();
                    }
                }

                private void OnDestroy()
                {
                    if (destroyEvent != null)
                    {
                        destroyEvent();
                    }
                }

                private void OnDrawGizmos()
                {
                    if (onDrawGizmosEvent != null)
                    {
                        onDrawGizmosEvent();
                    }
                }

                private void OnApplicationPause(bool pauseStatus)
                {
                    if (onApplicationPause != null)
                    {
                        onApplicationPause(pauseStatus);
                    }
                }

                public void AddLateUpdateListener(UnityAction fun)
                {
                    lateUpdateEvent += fun;
                }

                public void RemoveLateUpdateListener(UnityAction fun)
                {
                    lateUpdateEvent -= fun;
                }

                public void AddFixedUpdateListener(UnityAction fun)
                {
                    fixedUpdateEvent += fun;
                }

                public void RemoveFixedUpdateListener(UnityAction fun)
                {
                    fixedUpdateEvent -= fun;
                }

                public void AddUpdateListener(UnityAction fun)
                {
                    updateEvent += fun;
                }

                public void RemoveUpdateListener(UnityAction fun)
                {
                    updateEvent -= fun;
                }
                
                public void AddDestroyListener(UnityAction fun)
                {
                    destroyEvent += fun;
                }

                public void RemoveDestroyListener(UnityAction fun)
                {
                    destroyEvent -= fun;
                }
                
                public void AddOnDrawGizmosListener(UnityAction fun)
                {
                    onDrawGizmosEvent += fun;
                }

                public void RemoveOnDrawGizmosListener(UnityAction fun)
                {
                    onDrawGizmosEvent -= fun;
                }
                
                public void AddOnApplicationPauseListener(UnityAction<bool> fun)
                {
                    onApplicationPause += fun;
                }

                public void RemoveOnApplicationPauseListener(UnityAction<bool> fun)
                {
                    onApplicationPause -= fun;
                }

                public void Release()
                {
                    updateEvent = null;
                    fixedUpdateEvent = null;
                    lateUpdateEvent = null;
                    onDrawGizmosEvent = null;
                    destroyEvent = null;
                    onApplicationPause = null;
                }
            }
        }
    }
}