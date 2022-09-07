using System;
using System.Collections.Generic;
using System.Reflection;

namespace TEngine.Runtime.UIModule
{
    public partial class UISys
    {
        /// <summary>
        /// UIController
        /// </summary>
        private readonly List<UIControllerBase> _listController = new List<UIControllerBase>();

        /// <summary>
        /// 自动注册UIController
        /// </summary>
        /// <remarks>nameSpace TEngine.Runtime.UIModule</remarks>
        private void RegisterAllController()
        {
            Type handlerBaseType = typeof(UIControllerBase);
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                if (!types[i].IsClass || types[i].IsAbstract)
                {
                    continue;
                }
                if (types[i].BaseType == handlerBaseType)
                {
                    UIControllerBase controller = (UIControllerBase)Activator.CreateInstance(types[i]);
                    AddController(controller);
                }
            }
        }

        private void AddController(UIControllerBase controller)
        {
            for (int i = 0; i < _listController.Count; i++)
            {
                var type = _listController[i].GetType();

                if (type == controller.GetType())
                {
                    Log.Error(Utility.Text.Format("repeat controller type: {0}", controller.GetType()));
                    return;
                }
            }
            
            _listController.Add(controller);
            
            controller.RegisterUIEvent();
        }

        public void AddController<T>() where T : UIControllerBase, new()
        {
            for (int i = 0; i < _listController.Count; i++)
            {
                var type = _listController[i].GetType();

                if (type == typeof(T))
                {
                    Log.Error(Utility.Text.Format("repeat controller type: {0}", typeof(T).Name));
                    return;
                }
            }

            var controller = new T();

            _listController.Add(controller);

            controller.RegisterUIEvent();
        }
    }
}