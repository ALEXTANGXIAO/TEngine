using System.Collections.Generic;
using TEngine.Runtime.Entity;
using UnityEngine;

namespace TEngine.Runtime.Actor
{
    public class ActorEntity : EntityLogicEx
    {
        protected override void OnShow(object userData)
        {
            base.OnInit(userData);

            var entityData = (EntityData)userData;

            GameActor actor = (GameActor)entityData.UserData;
            
            actor.Event.SendEvent(StringId.StringToHash("ActorEntityOnShow"),gameObject);
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            
        }
    }
    
    /// <summary>
    /// 玩家Actor，把Actor具象化成玩家
    /// </summary>
    public class PlayerActor:GameActor
    {
        public override int GetActorType()
        {
            return ActorType.ActorPlayer;
        }

        public override void Awake()
        {
            base.Awake();
        }

        public override void OnInit()
        {
            base.OnInit();
        }
    }

    /// <summary>
    /// Animator组件
    /// </summary>
    public class AnimatorComponent : ActorComponent
    {
        private Animator _animator;
        
        protected override void Awake()
        {
            base.Awake();
            
            _animator = OwnActor.gameObject.AddComponent<Animator>();
        }
    }
    
    /// <summary>
    /// 模型组件
    /// </summary>
    public class ModelComponent : ActorComponent
    {
        private GameObject _model;
        
        /// <summary>
        /// Awake
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            RegisterEvent();

            InitModel();

            BindOwnActor();
        }

        /// <summary>
        /// 注册组件事件
        /// </summary>
        private void RegisterEvent()
        {
            OwnActor.Event.AddEventListener<GameObject>(StringId.StringToHash("ActorEntityOnShow"),OnLoadModel,OwnActor);
        }

        private void InitModel()
        {
            //普通创建实体
            // var obj = TResources.Load("Capsule");
            // _model = Object.Instantiate(obj);
            
            //通过Entity创建实体
            PlayEntityMgr.Instance.CreatePlayerEntity(OwnActor,"Capsule",Vector3.zero, Quaternion.identity);
        }

        public void OnLoadModel(GameObject gameObject)
        {
            _model = gameObject;
            
            BindOwnActor();
        }

        private void BindOwnActor()
        {
            if (_model == null)
            {
                return;
            }
            
            _model.transform.SetParent(OwnActor.gameObject.transform);
        }
    }

    /// <summary>
    /// 状态机组件
    /// </summary>
    public class FsmComponent : ActorComponent
    {
        public IFsm<FsmComponent> Fsm;

        protected override void Awake()
        {
            base.Awake();
            
            List<FsmState<FsmComponent>> stateList = new List<FsmState<FsmComponent>>() { IdleState.Create(), MoveState.Create() };
        
            Fsm = FsmManager.Instance.CreateFsm(OwnActor.ActorId.ToString(), this, stateList);
        
            Fsm.Start<IdleState>();
        }

        #region 状态机
        public class IdleState : FsmState<FsmComponent>, IMemory
        {
            //触发移动的指令列表
            private static KeyCode[] MOVE_COMMANDS =
            {
                KeyCode.LeftArrow, 
                KeyCode.RightArrow, 
                KeyCode.UpArrow, 
                KeyCode.DownArrow,
                KeyCode.A,
                KeyCode.W,
                KeyCode.S,
                KeyCode.D
            };

            protected override void OnInit(IFsm<FsmComponent> fsm)
            {
                base.OnInit(fsm);
            }

            protected override void OnEnter(IFsm<FsmComponent> fsm)
            {
                Log.Warning("OnEnter IdleState");
                base.OnEnter(fsm);
            }

            protected override void OnUpdate(IFsm<FsmComponent> fsm, float elapseSeconds, float realElapseSeconds)
            {
                base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

                foreach (var command in MOVE_COMMANDS)
                {
                    //触发任何一个移动指令时
                    if (Input.GetKeyDown(command))
                    {
                        //记录这个移动指令
                        fsm.SetData<int>("MoveCommand", (int)command);
                        //切换到移动状态
                        ChangeState<MoveState>(fsm);
                    }
                }
            }

            protected override void OnLeave(IFsm<FsmComponent> fsm, bool isShutdown)
            {
                base.OnLeave(fsm, isShutdown);
            }

            protected override void OnDestroy(IFsm<FsmComponent> fsm)
            {
                base.OnDestroy(fsm);
            }

            public static IdleState Create()
            {
                IdleState state = MemoryPool.Acquire<IdleState>();
                return state;
            }

            public void Clear()
            {
                //此类无状态记录，Clear为空实现
            }
        }
        
        public class MoveState : FsmState<FsmComponent>, IMemory
        {
            private static readonly float EXIT_TIME = 0.01f;
            private float exitTimer;
            private KeyCode moveCommand;

            protected override void OnInit(IFsm<FsmComponent> fsm)
            {
                base.OnInit(fsm);
            }

            protected override void OnEnter(IFsm<FsmComponent> fsm)
            {
                base.OnEnter(fsm);
                
                //进入移动状态时，获取移动指令数据
                moveCommand = (KeyCode)(int)fsm.GetData<int>("MoveCommand");
                
                Log.Warning("OnEnter MoveState" + moveCommand);

            }

            protected override void OnUpdate(IFsm<FsmComponent> fsm, float elapseSeconds, float realElapseSeconds)
            {
                base.OnUpdate(fsm, elapseSeconds, realElapseSeconds);

                //计时器累计时间
                exitTimer += elapseSeconds;

                //switch(moveCommand)
                //{
                //根据移动方向指令向对应方向移动
                //}

                if (moveCommand != 0)
                {
                    exitTimer = 0;
                }

                //达到指定时间后
                if (exitTimer > EXIT_TIME)
                {
                    //切换回空闲状态
                    ChangeState<IdleState>(fsm);
                }
            }

            protected override void OnLeave(IFsm<FsmComponent> fsm, bool isShutdown)
            {
                base.OnLeave(fsm, isShutdown);

                //推出移动状态时，把计时器清零
                exitTimer = 0;
                //清空移动指令
                moveCommand = KeyCode.None;
                fsm.RemoveData("MoveCommand");
            }

            protected override void OnDestroy(IFsm<FsmComponent> fsm)
            {
                base.OnDestroy(fsm);
            }

            public static MoveState Create()
            {
                MoveState state = MemoryPool.Acquire<MoveState>();
                return state;
            }

            public void Clear()
            {
                //还原状态内数据
                exitTimer = 0;
                moveCommand = KeyCode.None;
            }
        }

        #endregion
        
    }
}