using System.Collections.Generic;
using System.Linq;
using TEngine;

namespace GameLogic.BattleDemo
{
    /// <summary>
    /// 实体类的Buff管理。
    /// </summary>
    public class BuffComponent:EntityLogicComponent
    {
        private readonly Dictionary<int, BufferItem> _allBuff = new Dictionary<int, BufferItem>();
        private readonly List<BufferItem> _listBuff = new List<BufferItem>();


        public override void Dispose()
        {
            foreach (var bufferItem in _listBuff)
            {
                BufferItem.Release(bufferItem);
            }
            _listBuff.Clear();
            _allBuff.Clear();
            base.Dispose();
        }

        /// <summary>
        /// 增加Buff。
        /// </summary>
        /// <param name="buffId">BuffId。</param>
        /// <param name="caster">施法者。</param>
        /// <param name="addStackNum">增加层数。</param>
        /// <param name="skillId">技能Id。</param>
        /// <returns></returns>
        public bool AddBuff(int buffId, EntityLogic caster, int addStackNum = 1, uint skillId = 0)
        {
            BufferItem bufferItem = BufferItem.Alloc(buffId);
            if (bufferItem == null)
            {
                return false;
            }
            RefreshBuffAttr();
            UpdateBuffState();
            _allBuff.Add(buffId, bufferItem);
            _listBuff.Add(bufferItem);
            return true;
        }

        /// <summary>
        /// 移除Buff。
        /// </summary>
        /// <param name="buffID">BuffID。</param>
        /// <param name="caster">移除施放来源。</param>
        public void RmvBuff(int buffID, EntityLogic caster)
        {
            if (_allBuff.TryGetValue(buffID, out BufferItem buffItem))
            {
                RemoveBuffFromList(buffItem);
                RefreshBuffAttr();
                UpdateBuffState();
            }
        }
        
        private void RemoveBuffFromList(BufferItem buffItem)
        {
            Log.Info("remove buff: {0}", buffItem.BuffID);
            BufferItem.Release(buffItem);
            _allBuff.Remove(buffItem.BuffID);
            _listBuff.Remove(buffItem);
        }
        
        /// <summary>
        /// 刷新Buff带来的属性。
        /// </summary>
        private void RefreshBuffAttr()
        {
        }

        /// <summary>
        /// 刷新Buff改变的状态。
        /// </summary>
        private void UpdateBuffState()
        {
        }
    }
}