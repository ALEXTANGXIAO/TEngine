using System.Collections.Generic;
using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 数据中心模块。
    /// </summary>
    public class DataCenterSys : BaseLogicSys<DataCenterSys>
    {
        /// <summary>
        /// 子模块集合。
        /// </summary>
        private readonly List<IDataCenterModule> _listModule = new List<IDataCenterModule>();
        
        /// <summary>
        /// 初始化数据中心。
        /// </summary>
        /// <returns></returns>
        public override bool OnInit()
        {
            InitModule();
            InitOtherModule();
            return true;
        }
        
        /// <summary>
        /// 初始化数据中心接口。
        /// </summary>
        /// <param name="module"></param>
        public void InitModule(IDataCenterModule module)
        {
            if (_listModule.Contains(module))
            {
                return;
            }
            module.Init();
            _listModule.Add(module);
        }
        
        /// <summary>
        /// 初始化数据中心模块。
        /// </summary>
        void InitModule()
        {
            InitModule(PlayerNetSys.Instance);
        }

        /// <summary>
        /// 初始化数据中心其他模块。
        /// <remarks>优先级低于InitModule。</remarks>
        /// </summary>
        void InitOtherModule()
        {

        }
        
        /// <summary>
        /// 帧更新驱动。
        /// </summary>
        public override void OnUpdate()
        {
            var listModule = _listModule;
            foreach (var module in listModule)
            {
                TProfiler.BeginSample(module.GetType().FullName);
                module.OnUpdate();
                TProfiler.EndSample();
            }
        }
    }
}