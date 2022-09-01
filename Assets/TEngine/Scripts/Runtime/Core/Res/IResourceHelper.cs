using System;
using UnityEngine;

namespace TEngine.Runtime
{
    /// <summary>
    /// 游戏资源加载辅助器
    /// </summary>
    public interface IResourceHelper
    {
        /// <summary>
        /// 同步加载GameObject
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        GameObject Load(string path);

        /// <summary>
        /// 同步加载GameObject
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        GameObject Load(string path, Transform parent);

        /// <summary>
        /// 同步加载泛型
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Load<T>(string path) where T : UnityEngine.Object;

        /// <summary>
        /// 异步加载GameObject
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callBack"></param>
        void LoadAsync(string path, Action<GameObject> callBack);

        /// <summary>
        /// 异步加载泛型
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callBack"></param>
        /// <param name="withSubAsset"></param>
        /// <typeparam name="T"></typeparam>
        void LoadAsync<T>(string path, Action<T> callBack, bool withSubAsset = false) where T : class;
    }

    /// <summary>
    /// 游戏资源加载辅助器基类
    /// </summary>
    public abstract class ResourceHelperBase : UnityEngine.MonoBehaviour, IResourceHelper
    {
        /// <summary>
        /// 同步加载GameObject
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public abstract GameObject Load(string path);

        /// <summary>
        /// 同步加载GameObject
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public abstract GameObject Load(string path, Transform parent);

        /// <summary>
        /// 同步加载泛型
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract T Load<T>(string path) where T : UnityEngine.Object;

        /// <summary>
        /// 异步加载GameObject
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callBack"></param>
        public abstract void LoadAsync(string path, Action<GameObject> callBack);

        /// <summary>
        /// 异步加载泛型
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callBack"></param>
        /// <param name="withSubAsset"></param>
        /// <typeparam name="T"></typeparam>
        public abstract void LoadAsync<T>(string path, Action<T> callBack, bool withSubAsset = false) where T : class;
    }
}