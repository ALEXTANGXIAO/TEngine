using System.Collections.Generic;

namespace TEngine.Runtime.UIModule
{
    public partial class UISys
    {
        private List<IUIController> m_listController = new List<IUIController>();

        public void RegisterAllController()
        {
            //AddController<LoadingUIController>();
        }

        private void AddController<T>() where T : IUIController, new()
        {
            for (int i = 0; i < m_listController.Count; i++)
            {
                var type = m_listController[i].GetType();

                if (type == typeof(T))
                {
                    Log.Error(Utility.Text.Format("repeat controller type: {0}", typeof(T).Name));
                    return;
                }
            }

            var controller = new T();

            m_listController.Add(controller);

            controller.RegisterUIEvent();
        }
    }
}