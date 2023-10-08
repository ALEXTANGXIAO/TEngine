using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 有限状态机模块。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class FsmModule : Module
    {
        private IFsmManager _fsmManager = null;

        /// <summary>
        /// 获取有限状态机数量。
        /// </summary>
        public int Count => _fsmManager.Count;

        /// <summary>
        /// 游戏框架模块初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _fsmManager = ModuleImpSystem.GetModule<IFsmManager>();
            if (_fsmManager == null)
            {
                Log.Fatal("FSM manager is invalid.");
                return;
            }
        }

        /// <summary>
        /// 检查是否存在有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <returns>是否存在有限状态机。</returns>
        public bool HasFsm<T>() where T : class
        {
            return _fsmManager.HasFsm<T>();
        }

        /// <summary>
        /// 检查是否存在有限状态机。
        /// </summary>
        /// <param name="ownerType">有限状态机持有者类型。</param>
        /// <returns>是否存在有限状态机。</returns>
        public bool HasFsm(Type ownerType)
        {
            return _fsmManager.HasFsm(ownerType);
        }

        /// <summary>
        /// 检查是否存在有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="name">有限状态机名称。</param>
        /// <returns>是否存在有限状态机。</returns>
        public bool HasFsm<T>(string name) where T : class
        {
            return _fsmManager.HasFsm<T>(name);
        }

        /// <summary>
        /// 检查是否存在有限状态机。
        /// </summary>
        /// <param name="ownerType">有限状态机持有者类型。</param>
        /// <param name="name">有限状态机名称。</param>
        /// <returns>是否存在有限状态机。</returns>
        public bool HasFsm(Type ownerType, string name)
        {
            return _fsmManager.HasFsm(ownerType, name);
        }

        /// <summary>
        /// 获取有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <returns>要获取的有限状态机。</returns>
        public IFsm<T> GetFsm<T>() where T : class
        {
            return _fsmManager.GetFsm<T>();
        }

        /// <summary>
        /// 获取有限状态机。
        /// </summary>
        /// <param name="ownerType">有限状态机持有者类型。</param>
        /// <returns>要获取的有限状态机。</returns>
        public FsmBase GetFsm(Type ownerType)
        {
            return _fsmManager.GetFsm(ownerType);
        }

        /// <summary>
        /// 获取有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="name">有限状态机名称。</param>
        /// <returns>要获取的有限状态机。</returns>
        public IFsm<T> GetFsm<T>(string name) where T : class
        {
            return _fsmManager.GetFsm<T>(name);
        }

        /// <summary>
        /// 获取有限状态机。
        /// </summary>
        /// <param name="ownerType">有限状态机持有者类型。</param>
        /// <param name="name">有限状态机名称。</param>
        /// <returns>要获取的有限状态机。</returns>
        public FsmBase GetFsm(Type ownerType, string name)
        {
            return _fsmManager.GetFsm(ownerType, name);
        }

        /// <summary>
        /// 获取所有有限状态机。
        /// </summary>
        public FsmBase[] GetAllFsms()
        {
            return _fsmManager.GetAllFsms();
        }

        /// <summary>
        /// 获取所有有限状态机。
        /// </summary>
        /// <param name="results">所有有限状态机。</param>
        public void GetAllFsms(List<FsmBase> results)
        {
            _fsmManager.GetAllFsms(results);
        }

        /// <summary>
        /// 创建有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="owner">有限状态机持有者。</param>
        /// <param name="states">有限状态机状态集合。</param>
        /// <returns>要创建的有限状态机。</returns>
        public IFsm<T> CreateFsm<T>(T owner, params FsmState<T>[] states) where T : class
        {
            return _fsmManager.CreateFsm(owner, states);
        }

        /// <summary>
        /// 创建有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="name">有限状态机名称。</param>
        /// <param name="owner">有限状态机持有者。</param>
        /// <param name="states">有限状态机状态集合。</param>
        /// <returns>要创建的有限状态机。</returns>
        public IFsm<T> CreateFsm<T>(string name, T owner, params FsmState<T>[] states) where T : class
        {
            return _fsmManager.CreateFsm(name, owner, states);
        }

        /// <summary>
        /// 创建有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="owner">有限状态机持有者。</param>
        /// <param name="states">有限状态机状态集合。</param>
        /// <returns>要创建的有限状态机。</returns>
        public IFsm<T> CreateFsm<T>(T owner, List<FsmState<T>> states) where T : class
        {
            return _fsmManager.CreateFsm(owner, states);
        }

        /// <summary>
        /// 创建有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="name">有限状态机名称。</param>
        /// <param name="owner">有限状态机持有者。</param>
        /// <param name="states">有限状态机状态集合。</param>
        /// <returns>要创建的有限状态机。</returns>
        public IFsm<T> CreateFsm<T>(string name, T owner, List<FsmState<T>> states) where T : class
        {
            return _fsmManager.CreateFsm(name, owner, states);
        }

        /// <summary>
        /// 销毁有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <returns>是否销毁有限状态机成功。</returns>
        public bool DestroyFsm<T>() where T : class
        {
            return _fsmManager.DestroyFsm<T>();
        }

        /// <summary>
        /// 销毁有限状态机。
        /// </summary>
        /// <param name="ownerType">有限状态机持有者类型。</param>
        /// <returns>是否销毁有限状态机成功。</returns>
        public bool DestroyFsm(Type ownerType)
        {
            return _fsmManager.DestroyFsm(ownerType);
        }

        /// <summary>
        /// 销毁有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="name">要销毁的有限状态机名称。</param>
        /// <returns>是否销毁有限状态机成功。</returns>
        public bool DestroyFsm<T>(string name) where T : class
        {
            return _fsmManager.DestroyFsm<T>(name);
        }

        /// <summary>
        /// 销毁有限状态机。
        /// </summary>
        /// <param name="ownerType">有限状态机持有者类型。</param>
        /// <param name="name">要销毁的有限状态机名称。</param>
        /// <returns>是否销毁有限状态机成功。</returns>
        public bool DestroyFsm(Type ownerType, string name)
        {
            return _fsmManager.DestroyFsm(ownerType, name);
        }

        /// <summary>
        /// 销毁有限状态机。
        /// </summary>
        /// <typeparam name="T">有限状态机持有者类型。</typeparam>
        /// <param name="fsm">要销毁的有限状态机。</param>
        /// <returns>是否销毁有限状态机成功。</returns>
        public bool DestroyFsm<T>(IFsm<T> fsm) where T : class
        {
            return _fsmManager.DestroyFsm(fsm);
        }

        /// <summary>
        /// 销毁有限状态机。
        /// </summary>
        /// <param name="fsm">要销毁的有限状态机。</param>
        /// <returns>是否销毁有限状态机成功。</returns>
        public bool DestroyFsm(FsmBase fsm)
        {
            return _fsmManager.DestroyFsm(fsm);
        }
    }
}
