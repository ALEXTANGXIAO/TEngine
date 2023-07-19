using GameConfig.Battle;
using TEngine;

namespace GameLogic.BattleDemo
{
    /// <summary>
    /// Buff实例。
    /// </summary>
    public class BufferItem:IMemory
    {
        /// <summary>
        /// BuffId。
        /// </summary>
        public int BuffID => BuffConfig?.BuffID ?? 0;

        /// <summary>
        /// BUff配置表。
        /// </summary>
        public BuffConfig BuffConfig { private set; get; }
        
        /// <summary>
        /// 清理内存。
        /// </summary>
        public void Clear()
        {
            BuffConfig = null;
        }

        /// <summary>
        /// 生成Buff实例。
        /// </summary>
        /// <param name="buffId">buffId。</param>
        /// <returns>Buff实例。</returns>
        public static BufferItem Alloc(int buffId)
        {
            Log.Debug($"Alloc buffItem buffId:{buffId}");
            BuffConfig buffConfig = ConfigLoader.Instance.Tables.TbBuff.Get(buffId);
            if (buffConfig == null)
            {
                Log.Warning($"Alloc buffItem Failed ! buffId:{buffId}");
                return null;
            }

            BufferItem ret = MemoryPool.Acquire<BufferItem>();
            ret.BuffConfig = buffConfig;
            return ret;
        }
        
        /// <summary>
        /// 释放Buff实例。
        /// </summary>
        /// <param name="bufferItem"></param>
        public static void Release(BufferItem bufferItem)
        {
            if (bufferItem == null)
            {
                return;
            }
            MemoryPool.Release(bufferItem);
        }
    }
}