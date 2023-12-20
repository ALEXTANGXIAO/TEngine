using TEngine;

namespace GameLogic
{
    /// <summary>
    /// 示意UI层事件。
    /// <remarks> 优化抛出事件，通过接口约束事件参数。</remarks>
    /// <remarks> example: GameEvent.Get<ILoginUI>().OnRoleLogin(isReconnect); </remarks>
    /// </summary>
    [EventInterface(EEventGroup.GroupUI)]
    public interface ILoginUI
    {
        public void OnRoleLogin(bool isReconnect);

        public void OnRoleLoginOut();
    }
}