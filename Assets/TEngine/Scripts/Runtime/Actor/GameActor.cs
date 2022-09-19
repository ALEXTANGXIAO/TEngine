using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TEngine.Runtime.Actor
{
    /// <summary>
    /// 游戏对象（GameActor）
    /// </summary>
    public abstract partial class GameActor
    {
        #region Propreties
        public uint ActorId { get; set; }

        public bool IsCreated { get; set; }
        
        public byte ActorSide;
        
        public abstract int GetActorType();

        public bool IsDestroyed { get; set; }

        public string Name = string.Empty;
        
        private ActorEventDispatcher _event = new ActorEventDispatcher();

        public ActorEventDispatcher Event => _event;

        private GameObject _gameObject;

        public GameObject gameObject => _gameObject;
        
        private float _visibleTime = 0;
        
        private bool _visible;
        
        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (_gameObject == null)
                {
                    return;
                }

                if (_visible != value)
                {
                    _visible = value && CheckActorCanVisible();
                    if (_visible && _visibleTime <= 0.0001f)
                    {
                        _visibleTime = Time.time;
                    }
                    Event.SendEvent(ActorEventType.ModelVisibleChange, _visible);
                }
            }
        }
        
        public virtual bool CheckActorCanVisible()
        {
            return true;
        }
        
        public string GetGameObjectName()
        {
#if UNITY_EDITOR
            return string.Format("[{0}][{1}][{2}]", ActorId, GetActorType(), GetActorName());
#else
            return "GameActor";
#endif
        }
        
        public virtual string GetActorName()
        {
            return "UNNAMED";
        }
        #endregion
        
        #region Transform
        public Transform transform
        {
            get
            {
                if (gameObject == null)
                {
                    throw new Exception("Runtime GameActor gameObject is Null");
                }
                
                return gameObject.transform;
            }
        }

        public Vector3 Position
        {
            get
            {
                if (transform == null)
                {
                    throw new Exception("Runtime GameActor transform is Null");
                }

                return transform.position;
            }
            set
            {
                if (transform == null)
                {
                    throw new Exception("Runtime GameActor transform is Null");
                }

                transform.position = value;
            }
        }

        public Vector3 Forward
        {
            get
            {
                if (transform == null)
                {
                    throw new Exception("Runtime GameActor transform is Null");
                }

                return transform.forward;
            }
            set
            {
                if (transform == null)
                {
                    throw new Exception("Runtime GameActor transform is Null");
                }

                transform.forward = value;
            }
        }
        
        public Vector3 LocalScale
        {
            get
            {
                if (transform == null)
                {
                    throw new Exception("Runtime GameActor transform is Null");
                }

                return transform.localScale;
            }
            set
            {
                if (transform == null)
                {
                    throw new Exception("Runtime GameActor transform is Null");
                }

                transform.localScale = value;
            }
        }
        
        public Quaternion Rotation
        {
            get
            {
                if (transform != null)
                {
                    return transform.rotation;
                }

                return Quaternion.identity;
            }
            set
            {
                if (transform != null)
                {
                    transform.rotation = value;
                }
            }
        }
        #endregion

        #region Init
        internal void Init()
        {
            Awake();
            BaseInit();
            OnInit();
            AfterInit();
        }

        public virtual void Awake()
        {
            
        }
        
        public virtual void OnInit()
        {
            
        }
        
        public virtual void AfterInit()
        {
            
        }
        
        protected virtual GameObject CreateGameObject()
        {
            return ActorManager.Instance.CreateGameObject(this);
        }

        protected void BaseInit()
        {
            if (_gameObject == null)
            {
                _visible = false;

                _gameObject = CreateGameObject();
                
                ActorManager.Instance.BindGameActorGo(this,_gameObject);
            }
            InitExt();
        }
        

        #endregion

        #region Methods

        public void Destroy()
        {
            Visible = false;

            _isDestroying = true;

            BeforeDestroyAllComponent();

            DestroyAllComponent();

            if (gameObject != null)
            {
                Object.Destroy(_gameObject);
                
                _gameObject = null;
            }

            IsDestroyed = true;
            _isDestroying = false;
        }
        #endregion

        #region Expand

        #region Base
        public static bool operator ==(GameActor obj1, GameActor obj2)
        {
            if (ReferenceEquals(obj1, obj2))
            {
                return true;
            }

            bool isObj1Null = ReferenceEquals(obj1, null) || obj1.IsDestroyed;
            bool isObj2Null = ReferenceEquals(obj2, null) || obj2.IsDestroyed;
            return isObj1Null && isObj2Null;
        }

        public static bool operator !=(GameActor obj1, GameActor obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj2)
        {
            bool isObj1Null = IsDestroyed;
            bool isObj2Null = ReferenceEquals(obj2, null);
            if (isObj1Null && isObj2Null)
            {
                return true;
            }

            return ReferenceEquals(this, obj2);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion

        #endregion
    }
}