using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine
{
    public class HotfixComponent : ECSComponent,IUpdate
    {
        public object[] Values;
        public Action OnAwake, OnUpdate, OnDestroyExt;

        public override void Awake()
        {
            OnAwake?.Invoke();
        }

        void IUpdate.Update()
        {
            OnUpdate?.Invoke();
        }

        public override void OnDestroy()
        {
            OnDestroyExt?.Invoke();
            OnAwake = null;
            OnUpdate = null;
            OnDestroyExt = null;
        }
    }
}

