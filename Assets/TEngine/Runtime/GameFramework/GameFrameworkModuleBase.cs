using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 游戏框架模块抽象类。
    /// </summary>
    public abstract class GameFrameworkModuleBase : MonoBehaviour
    {
        /// <summary>
        /// 游戏框架模块初始化。
        /// </summary>
        protected virtual void Awake()
        {
            GameModuleSystem.RegisterModule(this);
        }
    }
}
