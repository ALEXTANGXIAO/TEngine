using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace TEngine
{
    /// <summary>
    /// 流程管理模块。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ProcedureModule : Module
    {
        private IProcedureManager _procedureManager = null;
        private ProcedureBase _entranceProcedure = null;

        [SerializeField] private string[] availableProcedureTypeNames = null;

        [SerializeField] private string entranceProcedureTypeName = null;

        /// <summary>
        /// 获取当前流程。
        /// </summary>
        public ProcedureBase CurrentProcedure
        {
            get
            {
                if (_procedureManager == null)
                {
                    return null;
                }

                return _procedureManager.CurrentProcedure;
            }
        }

        /// <summary>
        /// 获取当前流程持续时间。
        /// </summary>
        public float CurrentProcedureTime
        {
            get
            {
                if (_procedureManager == null)
                {
                    return 0f;
                }

                return _procedureManager.CurrentProcedureTime;
            }
        }

        /// <summary>
        /// 游戏框架模块初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _procedureManager = ModuleImpSystem.GetModule<IProcedureManager>();
            if (_procedureManager == null)
            {
                Log.Fatal("Procedure manager is invalid.");
            }
        }

        private IEnumerator Start()
        {
            ProcedureBase[] procedures = new ProcedureBase[availableProcedureTypeNames.Length];
            for (int i = 0; i < availableProcedureTypeNames.Length; i++)
            {
                Type procedureType = Utility.Assembly.GetType(availableProcedureTypeNames[i]);
                if (procedureType == null)
                {
                    Log.Error("Can not find procedure type '{0}'.", availableProcedureTypeNames[i]);
                    yield break;
                }

                procedures[i] = (ProcedureBase)Activator.CreateInstance(procedureType);
                if (procedures[i] == null)
                {
                    Log.Error("Can not create procedure instance '{0}'.", availableProcedureTypeNames[i]);
                    yield break;
                }

                if (entranceProcedureTypeName == availableProcedureTypeNames[i])
                {
                    _entranceProcedure = procedures[i];
                }
            }

            if (_entranceProcedure == null)
            {
                Log.Error("Entrance procedure is invalid.");
                yield break;
            }

            _procedureManager.Initialize(ModuleImpSystem.GetModule<IFsmManager>(), procedures);

            yield return new WaitForEndOfFrame();

            _procedureManager.StartProcedure(_entranceProcedure.GetType());
        }

        /// <summary>
        /// 是否存在流程。
        /// </summary>
        /// <typeparam name="T">要检查的流程类型。</typeparam>
        /// <returns>是否存在流程。</returns>
        public bool HasProcedure<T>() where T : ProcedureBase
        {
            return _procedureManager.HasProcedure<T>();
        }

        /// <summary>
        /// 是否存在流程。
        /// </summary>
        /// <param name="procedureType">要检查的流程类型。</param>
        /// <returns>是否存在流程。</returns>
        public bool HasProcedure(Type procedureType)
        {
            return _procedureManager.HasProcedure(procedureType);
        }

        /// <summary>
        /// 获取流程。
        /// </summary>
        /// <typeparam name="T">要获取的流程类型。</typeparam>
        /// <returns>要获取的流程。</returns>
        public ProcedureBase GetProcedure<T>() where T : ProcedureBase
        {
            return _procedureManager.GetProcedure<T>();
        }

        /// <summary>
        /// 获取流程。
        /// </summary>
        /// <param name="procedureType">要获取的流程类型。</param>
        /// <returns>要获取的流程。</returns>
        public ProcedureBase GetProcedure(Type procedureType)
        {
            return _procedureManager.GetProcedure(procedureType);
        }

        /// <summary>
        /// 重启流程。
        /// <remarks>默认使用第一个流程作为启动流程。</remarks>
        /// </summary>
        /// <param name="procedures">新的的流程。</param>
        /// <returns>是否重启成功。</returns>
        /// <exception cref="GameFrameworkException">重启异常。</exception>
        public bool RestartProcedure(params ProcedureBase[] procedures)
        {
            if (procedures == null || procedures.Length <= 0)
            {
                throw new GameFrameworkException("RestartProcedure Failed procedures is invalid.");
            }

            IFsmManager fsmManager = ModuleImpSystem.GetModule<IFsmManager>();

            if (!fsmManager.DestroyFsm<IProcedureManager>())
            {
                return false;
            }

            _procedureManager = null;
            _procedureManager = ModuleImpSystem.GetModule<IProcedureManager>();
            _procedureManager.Initialize(fsmManager, procedures);
            _procedureManager.StartProcedure(procedures[0].GetType());
            return true;
        }
    }
}