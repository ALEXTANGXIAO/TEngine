using TEngine;

namespace GameLogic
{
    public interface IDataCenterModule
    {
        void Init();

        void OnRoleLogin();
        
        void OnRoleLogout();

        void OnUpdate();

        void OnMainPlayerMapChange();
    }
    public class DataCenterModule<T> : IDataCenterModule where T : new()
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new T();
                    Log.Assert(_instance != null);
                }
                return _instance;
            }
        }

        public virtual void Init()
        {

        }
        
        public virtual void OnRoleLogin()
        {

        }

        public virtual void OnRoleLogout()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnMainPlayerMapChange()
        {

        }
    }
}