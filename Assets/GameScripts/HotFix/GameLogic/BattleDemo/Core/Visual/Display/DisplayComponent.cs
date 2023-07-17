using TEngine;

namespace GameLogic.BattleDemo
{
    /// <summary>
    /// 外观显示组件。
    /// </summary>
    public class DisplayComponent:EntityLogicComponent
    {
        public DisplayInfo displayInfo;

        public override void Dispose()
        {
            MemoryPool.Release(displayInfo);
            base.Dispose();
        }
    }
}