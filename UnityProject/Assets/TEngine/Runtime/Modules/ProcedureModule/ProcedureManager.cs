using System;

namespace TEngine
{
    /// <summary>
    /// 流程管理器。
    /// </summary>
    internal sealed class ProcedureManager : ModuleImp, IProcedureManager
    {
        private IFsmManager _fsmManager;
        private IFsm<IProcedureManager> _procedureFsm;

        /// <summary>
        /// 初始化流程管理器的新实例。
        /// </summary>
        public ProcedureManager()
        {
            _fsmManager = null;
            _procedureFsm = null;
        }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        internal override int Priority => -2;

        /// <summary>
        /// 获取当前流程。
        /// </summary>
        public ProcedureBase CurrentProcedure
        {
            get
            {
                if (_procedureFsm == null)
                {
                    throw new GameFrameworkException("You must initialize procedure first.");
                }

                return (ProcedureBase)_procedureFsm.CurrentState;
            }
        }

        /// <summary>
        /// 获取当前流程持续时间。
        /// </summary>
        public float CurrentProcedureTime
        {
            get
            {
                if (_procedureFsm == null)
                {
                    throw new GameFrameworkException("You must initialize procedure first.");
                }

                return _procedureFsm.CurrentStateTime;
            }
        }

        /// <summary>
        /// 流程管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理流程管理器。
        /// </summary>
        internal override void Shutdown()
        {
            if (_fsmManager != null)
            {
                if (_procedureFsm != null)
                {
                    _fsmManager.DestroyFsm(_procedureFsm);
                    _procedureFsm = null;
                }

                _fsmManager = null;
            }
        }

        /// <summary>
        /// 初始化流程管理器。
        /// </summary>
        /// <param name="fsmManager">有限状态机管理器。</param>
        /// <param name="procedures">流程管理器包含的流程。</param>
        public void Initialize(IFsmManager fsmManager, params ProcedureBase[] procedures)
        {
            if (fsmManager == null)
            {
                throw new GameFrameworkException("FSM manager is invalid.");
            }

            _fsmManager = fsmManager;
            _procedureFsm = _fsmManager.CreateFsm(this, procedures);
        }

        /// <summary>
        /// 开始流程。
        /// </summary>
        /// <typeparam name="T">要开始的流程类型。</typeparam>
        public void StartProcedure<T>() where T : ProcedureBase
        {
            if (_procedureFsm == null)
            {
                throw new GameFrameworkException("You must initialize procedure first.");
            }

            _procedureFsm.Start<T>();
        }

        /// <summary>
        /// 开始流程。
        /// </summary>
        /// <param name="procedureType">要开始的流程类型。</param>
        public void StartProcedure(Type procedureType)
        {
            if (_procedureFsm == null)
            {
                throw new GameFrameworkException("You must initialize procedure first.");
            }

            _procedureFsm.Start(procedureType);
        }

        /// <summary>
        /// 是否存在流程。
        /// </summary>
        /// <typeparam name="T">要检查的流程类型。</typeparam>
        /// <returns>是否存在流程。</returns>
        public bool HasProcedure<T>() where T : ProcedureBase
        {
            if (_procedureFsm == null)
            {
                throw new GameFrameworkException("You must initialize procedure first.");
            }

            return _procedureFsm.HasState<T>();
        }

        /// <summary>
        /// 是否存在流程。
        /// </summary>
        /// <param name="procedureType">要检查的流程类型。</param>
        /// <returns>是否存在流程。</returns>
        public bool HasProcedure(Type procedureType)
        {
            if (_procedureFsm == null)
            {
                throw new GameFrameworkException("You must initialize procedure first.");
            }

            return _procedureFsm.HasState(procedureType);
        }

        /// <summary>
        /// 获取流程。
        /// </summary>
        /// <typeparam name="T">要获取的流程类型。</typeparam>
        /// <returns>要获取的流程。</returns>
        public ProcedureBase GetProcedure<T>() where T : ProcedureBase
        {
            if (_procedureFsm == null)
            {
                throw new GameFrameworkException("You must initialize procedure first.");
            }

            return _procedureFsm.GetState<T>();
        }

        /// <summary>
        /// 获取流程。
        /// </summary>
        /// <param name="procedureType">要获取的流程类型。</param>
        /// <returns>要获取的流程。</returns>
        public ProcedureBase GetProcedure(Type procedureType)
        {
            if (_procedureFsm == null)
            {
                throw new GameFrameworkException("You must initialize procedure first.");
            }

            return (ProcedureBase)_procedureFsm.GetState(procedureType);
        }
    }
}