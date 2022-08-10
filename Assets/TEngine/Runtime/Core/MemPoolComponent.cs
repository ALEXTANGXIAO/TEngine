using System.Collections.Generic;
using UnityEngine;

namespace TEngine
{
    public class MemPoolComponent:UnitySingleton<MemPoolComponent>
    {
        public MemPoolMgr PoolMgr;

        protected override void OnLoad()
        {
            base.OnLoad();
            PoolMgr = MemPoolMgr.Instance;
        }

        public int Count => PoolMgr.Count;

        public List<IMemPoolBase> GetAllObjectPools()
        {
            return PoolMgr.GetAllObjectPools();
        }
    }
}
