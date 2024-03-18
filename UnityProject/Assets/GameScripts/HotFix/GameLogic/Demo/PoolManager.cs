using System;
using System.Collections.Generic;
using TEngine;
using UnityEngine;

namespace GameLogic
{
    public class PoolManager : MonoBehaviour
    {
        private static PoolManager _instance;

        public static PoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PoolManager>();
                }

                if (_instance == null)
                {
                    GameObject gameObject = new GameObject();
                    gameObject.name = nameof(PoolManager);
                    _instance = gameObject.AddComponent<PoolManager>();
                    _instance.poolRootObj = gameObject;
                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }

        [SerializeField] private GameObject poolRootObj;
        public Dictionary<string, GameObjectPoolData> gameObjectPoolDic = new Dictionary<string, GameObjectPoolData>();
        public Dictionary<string, ObjectPoolData> objectPoolDic = new Dictionary<string, ObjectPoolData>();

        public GameObject GetGameObject(string assetName, Transform parent = null)
        {
            GameObject obj = null;
            if (gameObjectPoolDic.TryGetValue(assetName, out var gameObjectPoolData) && gameObjectPoolData.poolQueue.Count > 0)
            {
                obj = gameObjectPoolData.GetObj(parent);
            }

            if (obj == null)
            {
                obj = GameModule.Resource.LoadGameObject(assetName, parent: parent);
                obj.name = assetName;
            }
            return obj;
        }

        public void PushGameObject(GameObject obj)
        {
            string objName = obj.name;
            if (gameObjectPoolDic.TryGetValue(objName, out var gameObjectPoolData))
            {
                gameObjectPoolData.PushObj(obj);
            }
            else
            {
                gameObjectPoolDic.Add(objName, new GameObjectPoolData(obj, poolRootObj));
            }
        }

        public T GetObject<T>() where T : class, new()
        {
            return CheckObjectCache<T>() ? (T)objectPoolDic[typeof(T).FullName].GetObj() : new T();
        }

        public void PushObject(object obj)
        {
            string fullName = obj.GetType().FullName;
            if (objectPoolDic.ContainsKey(fullName))
            {
                objectPoolDic[fullName].PushObj(obj);
            }
            else
            {
                objectPoolDic.Add(fullName, new ObjectPoolData(obj));
            }
        }

        private bool CheckObjectCache<T>()
        {
            string fullName = typeof(T).FullName;
            return fullName != null && objectPoolDic.ContainsKey(fullName) && objectPoolDic[fullName].poolQueue.Count > 0;
        }

        public void Clear(bool clearGameObject = true, bool clearCObject = true)
        {
            if (clearGameObject)
            {
                for (int index = 0; index < poolRootObj.transform.childCount; ++index)
                {
                    Destroy(poolRootObj.transform.GetChild(index).gameObject);
                }
                gameObjectPoolDic.Clear();
            }

            if (!clearCObject)
            {
                return;
            }
            objectPoolDic.Clear();
        }

        public void ClearAllGameObject() => Clear(clearCObject: false);

        public void ClearGameObject(string prefabName)
        {
            GameObject obj = poolRootObj.transform.Find(prefabName).gameObject;
            if (obj == null)
            {
                return;
            }

            Destroy(obj);
            gameObjectPoolDic.Remove(prefabName);
        }

        public void ClearGameObject(GameObject prefab) => ClearGameObject(prefab.name);

        public void ClearAllObject() => Clear(false);

        public void ClearObject<T>() => objectPoolDic.Remove(typeof(T).FullName);

        public void ClearObject(Type type) => objectPoolDic.Remove(type.FullName);
    }

    public class ObjectPoolData
    {
        public readonly Queue<object> poolQueue = new Queue<object>();

        public ObjectPoolData(object obj) => PushObj(obj);

        public void PushObj(object obj) => poolQueue.Enqueue(obj);

        public object GetObj() => poolQueue.Dequeue();
    }

    public class GameObjectPoolData
    {
        public readonly GameObject fatherObj;
        public readonly Queue<GameObject> poolQueue;

        public GameObjectPoolData(GameObject obj, GameObject poolRootObj)
        {
            fatherObj = new GameObject(obj.name);
            fatherObj.transform.SetParent(poolRootObj.transform);
            poolQueue = new Queue<GameObject>();
            PushObj(obj);
        }

        public GameObjectPoolData(GameObject fatherObj)
        {
            this.fatherObj = fatherObj;
        }

        public void PushObj(GameObject obj)
        {
            poolQueue.Enqueue(obj);
            obj.transform.SetParent(fatherObj.transform);
            obj.SetActive(false);
        }

        public GameObject GetObj(Transform parent = null)
        {
            GameObject go = poolQueue.Dequeue();
            go.SetActive(true);
            go.transform.SetParent(parent);
            if (parent == null)
            {
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            }

            return go;
        }
    }
}