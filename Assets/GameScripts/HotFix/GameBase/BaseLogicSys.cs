namespace TEngine
{
    /// <summary>
    /// 基础LogicSys,生命周期由TEngine实现，推荐给系统实现，
    /// 减少多余的Mono，保持系统层面只有一个Update。
    /// 用主Mono来驱动LogicSys的生命周期。
    /// </summary>
    /// <typeparam name="T">逻辑系统类型。</typeparam>
    public abstract class BaseLogicSys<T> : ILogicSys where T : new()
    {
        private static T _instance;

        public static bool HasInstance => _instance != null;

        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new T();
                }

                return _instance;
            }
        }

        #region virtual function
        public virtual bool OnInit()
        {
            if (null == _instance)
            {
                _instance = new T();
            }
            return true;
        }

        public virtual void OnStart()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnLateUpdate()
        {
        }

        public virtual void OnFixedUpdate()
        {
        }

        public virtual void OnRoleLogin()
        {
        }
        
        public virtual void OnRoleLogout()
        {
        }

        public virtual void OnDestroy()
        {
        }

        public virtual void OnDrawGizmos()
        {
        }

        public virtual void OnApplicationPause(bool pause)
        {
        }

        public virtual void OnMapChanged()
        {
        }
        #endregion
    }
}