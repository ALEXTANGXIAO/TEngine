#if TENGINE_UNITY
using TEngine.Core;
using UnityEngine;

namespace TEngine
{
    public class GameAppEntry : MonoBehaviour
    {
        /// <summary>
        /// 初始化框架
        /// </summary>
        public static Scene Initialize()
        {
            // 初始化框架
            ApplicationContext.Initialize();
            new GameObject("[TEngine.Unity]").AddComponent<GameAppEntry>();
            // 框架需要一个Scene来驱动、所以要创建一个Scene、后面所有的框架都会在这个Scene下
            // 也就是把这个Scene给卸载掉、框架的东西都会清除掉
            return Scene.Create("Unity");
        }

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
        
        private void Update()
        {
            ThreadSynchronizationContext.Main.Update();
            SingletonSystem.Update(); 
        }
        
        private void OnApplicationQuit()
        {
            EventSystem.Instance?.Publish(new OnAppClosed());
            ApplicationContext.Close();
        }
    }
}
#endif