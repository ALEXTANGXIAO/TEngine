using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TEngine
{
    /// <summary>
    /// 游戏入口。
    /// </summary>
    public static class GameEntry
    {
        private static readonly GameFrameworkLinkedList<GameFrameworkModuleBase> s_GameFrameworkModules = new GameFrameworkLinkedList<GameFrameworkModuleBase>();

        /// <summary>
        /// 游戏框架所在的场景编号。
        /// </summary>
        internal const int GameFrameworkSceneId = 0;

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <typeparam name="T">要获取的游戏框架模块类型。</typeparam>
        /// <returns>要获取的游戏框架模块。</returns>
        public static T GetModule<T>() where T : GameFrameworkModuleBase
        {
            return (T)GetModule(typeof(T));
        }

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <param name="type">要获取的游戏框架模块类型。</param>
        /// <returns>要获取的游戏框架模块。</returns>
        public static GameFrameworkModuleBase GetModule(Type type)
        {
            LinkedListNode<GameFrameworkModuleBase> current = s_GameFrameworkModules.First;
            while (current != null)
            {
                if (current.Value.GetType() == type)
                {
                    return current.Value;
                }

                current = current.Next;
            }

            return null;
        }

        /// <summary>
        /// 获取游戏框架模块。
        /// </summary>
        /// <param name="typeName">要获取的游戏框架模块类型名称。</param>
        /// <returns>要获取的游戏框架模块。</returns>
        public static GameFrameworkModuleBase GetModule(string typeName)
        {
            LinkedListNode<GameFrameworkModuleBase> current = s_GameFrameworkModules.First;
            while (current != null)
            {
                Type type = current.Value.GetType();
                if (type.FullName == typeName || type.Name == typeName)
                {
                    return current.Value;
                }

                current = current.Next;
            }

            return null;
        }

        /// <summary>
        /// 关闭游戏框架。
        /// </summary>
        /// <param name="shutdownType">关闭游戏框架类型。</param>
        public static void Shutdown(ShutdownType shutdownType)
        {
            Log.Info("Shutdown Game Framework ({0})...", shutdownType);
            Utility.Unity.Release();
            RootModule rootModule = GetModule<RootModule>();
            if (rootModule != null)
            {
                rootModule.Shutdown();
                rootModule = null;
            }

            s_GameFrameworkModules.Clear();

            if (shutdownType == ShutdownType.None)
            {
                return;
            }

            if (shutdownType == ShutdownType.Restart)
            {
                SceneManager.LoadScene(GameFrameworkSceneId);
                return;
            }

            if (shutdownType == ShutdownType.Quit)
            {
                Application.Quit();
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#endif
            }
        }

        /// <summary>
        /// 注册游戏框架模块。
        /// </summary>
        /// <param name="gameFrameworkModule">要注册的游戏框架模块。</param>
        internal static void RegisterModule(GameFrameworkModuleBase gameFrameworkModule)
        {
            if (gameFrameworkModule == null)
            {
                Log.Error("Game Framework component is invalid.");
                return;
            }

            Type type = gameFrameworkModule.GetType();

            LinkedListNode<GameFrameworkModuleBase> current = s_GameFrameworkModules.First;
            while (current != null)
            {
                if (current.Value.GetType() == type)
                {
                    Log.Error("Game Framework component type '{0}' is already exist.", type.FullName);
                    return;
                }

                current = current.Next;
            }

            s_GameFrameworkModules.AddLast(gameFrameworkModule);
        }
    }
}
