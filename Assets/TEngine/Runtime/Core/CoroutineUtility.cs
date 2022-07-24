using System.Collections;
using UnityEngine;

namespace TEngine
{
    public class CoroutineUtility
    {
        private static GameObject _entity;
        private static MonoBehaviour _behaviour;

        /// <summary>
        /// 开始协程
        /// </summary>
        /// <param name="routine">对应的迭代器</param>
        /// <returns></returns>
        public static Coroutine StartCoroutine(IEnumerator routine)
        {
            if (routine == null)
            {
                return null;
            }

            _MakeEntity();
            return _behaviour.StartCoroutine(routine);
        }

        /// <summary>
        /// 停止协程
        /// </summary>
        /// <param name="routine"></param>
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

        /// <summary>
        /// 停掉所有的协程
        /// </summary>
        public static void StopAllCoroutines()
        {
            if (_entity != null)
            {
                _behaviour.StopAllCoroutines();
            }
        }

        private static void _MakeEntity()
        {
            if (_entity != null)
            {
                return;
            }

            _entity = new GameObject("__coroutine__")
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

        private class MainBehaviour : MonoBehaviour
        {

        }
    }
}
