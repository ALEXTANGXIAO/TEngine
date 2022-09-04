using UnityEngine;

namespace TEngine.Runtime
{
    /// <summary>
    /// 具备Unity完整生命周期的单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UnitySingleton<T> : MonoBehaviour,IUnitySingleton where T : MonoBehaviour
    {

        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var ins = UnityEngine.Object.FindObjectOfType<T>();
                    if (ins != null)
                    {
                        ins.gameObject.name = typeof(T).Name;
                        _instance = ins;
                        SingletonMgr.Retain(ins.gameObject);
                        return Instance;
                    }

                    System.Type thisType = typeof(T);
                    string instName = thisType.Name;
                    GameObject go = SingletonMgr.GetGameObject(instName);
                    if (go == null)
                    {
                        go = GameObject.Find($"{instName}");
                        if (go == null)
                        {
                            go = new GameObject(instName);
                            go.transform.position = Vector3.zero;
                        }
                    }

                    _instance = go.GetComponent<T>();
                    if (_instance == null)
                    {
                        _instance = go.AddComponent<T>();
                    }

                    if (_instance == null)
                    {
                        TLogger.LogError($"Can't create UnitySingleton<{typeof(T)}>");
                    }
                }
                return _instance;
            }
        }

        public static T Active()
        {
            return Instance;
        }

        public static bool IsValid
        {
            get
            {
                return _instance != null;
            }
        }

        private bool CheckInstance()
        {
            if (this == Instance)
            {
                return true;
            }
            GameObject.Destroy(gameObject);
            return false;
        }

        protected virtual void OnLoad()
        {

        }

        public virtual void Awake()
        {
            if (CheckInstance())
            {
                UpdateInstance.Instance.Retain(this);
                OnLoad();
            }
#if UNITY_EDITOR
            TLogger.LogInfo($"UnitySingleton Instance:{typeof(T).Name}");
#endif
            GameObject tEngine = SingletonMgr.Root;
            if (tEngine != null)
            {
                this.gameObject.transform.SetParent(tEngine.transform);
            }
        }

        /// <summary>
        /// 获取游戏框架模块优先级。实现Interface
        /// </summary>
        /// <returns></returns>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public int GetPriority()
        {
            return Priority;
        }
        
        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public virtual int Priority
        {
            get
            {
                return 1;
            }
        }


        /// <summary>
        /// OnUpdate通过TEngine统一驱动,舍弃Unity的Update
        /// </summary>
        /// <param name="elapseSeconds"></param>
        /// <param name="realElapseSeconds"></param>
        public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            
        }

        public virtual void OnDestroy()
        {
            UpdateInstance.Instance.Release(this);
            Release();
        }

        public static void Release()
        {
            if (_instance != null)
            {
                SingletonMgr.Release(_instance.gameObject);
                _instance = null;
            }
        }
    }
}