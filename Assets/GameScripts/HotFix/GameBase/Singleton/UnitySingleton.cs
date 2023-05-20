using TEngine;
using UnityEngine;

namespace GameBase
{
    /// <summary>
    /// 具备Unity完整生命周期的单例。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class UnitySingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var ins = FindObjectOfType<T>();
                    if (ins != null)
                    {
                        var obj = ins.gameObject;
                        obj.name = typeof(T).Name;
                        _instance = ins;
                        SingletonMgr.Retain(obj);
                        return Instance;
                    }

                    System.Type thisType = typeof(T);
                    string instName = thisType.Name;
                    GameObject go = SingletonMgr.GetGameObject(instName);
                    if (go == null)
                    {
                        go = GameObject.Find($"[{instName}]");
                        if (go == null)
                        {
                            go = new GameObject($"[{instName}]")
                            {
                                transform =
                                {
                                    position = Vector3.zero
                                }
                            };
                        }
                    }

                    _instance = go.GetComponent<T>();
                    if (_instance == null)
                    {
                        _instance = go.AddComponent<T>();
                    }

                    if (_instance == null)
                    {
                        Log.Error($"Can't create UnitySingleton<{typeof(T)}>");
                    }
                }

                return _instance;
            }
        }

        public static T Active()
        {
            return Instance;
        }

        public static bool IsValid => _instance != null;

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
                OnLoad();
            }
#if UNITY_EDITOR
            Log.Debug($"UnitySingleton Instance:{typeof(T).Name}");
#endif
            GameObject tEngine = SingletonMgr.Root;
            if (tEngine != null)
            {
                this.gameObject.transform.SetParent(tEngine.transform);
            }
        }

        protected virtual void OnDestroy()
        {
            Release();
        }

        public static void Release()
        {
            if (_instance == null) return;
            SingletonMgr.Release(_instance.gameObject);
            _instance = null;
        }
    }
}