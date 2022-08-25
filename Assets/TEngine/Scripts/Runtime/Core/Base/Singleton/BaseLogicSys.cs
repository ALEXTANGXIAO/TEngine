namespace TEngine.Runtime
{
    /// <summary>
    /// 基础LogicSys,生命周期由TEngine实现，推荐给系统实现，
    /// 减少多余的Mono，保持系统层面只有一个Update
    /// 用TEngine的主Mono来驱动LogicSys的生命周期
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseLogicSys<T> : ILogicSys where T : new()
    {
        private static T m_Instance;

        public static bool HasInstance
        {
            get { return m_Instance != null; }
        }
        public static T Instance
        {
            get
            {
                if (null == m_Instance)
                {
                    m_Instance = new T();
                }

                return m_Instance;
            }
        }

        #region virtual function
        public virtual bool OnInit()
        {
            if (null == m_Instance)
            {
                m_Instance = new T();
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

        public virtual void OnDestroy()
        {
        }

        public virtual void OnPause()
        {
        }

        public virtual void OnResume()
        {
        }

        public virtual void OnDrawGizmos()
        {
        }

        public virtual void OnMapChanged()
        {
        }
        #endregion
    }

}