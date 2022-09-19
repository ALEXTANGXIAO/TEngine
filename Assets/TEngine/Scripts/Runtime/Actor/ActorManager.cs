using System.Collections.Generic;
using UnityEngine;


namespace TEngine.Runtime.Actor
{
    public partial class ActorType
    {
        public static int ActorNone = 1 << 0;
        public static int ActorPlayer = 1 << 1;
    }

    public delegate bool CheckActorDelegate(GameActor actor);

    public partial class ActorManager : BehaviourSingleton<ActorManager>
    {
        private const uint ClientGidStart = 1000000000;
        private const uint ClientGidEnd = 2000000000;
        private uint _clientGid = ClientGidStart;

        private Dictionary<uint, GameActor> _actorPool = new Dictionary<uint, GameActor>();
        private Dictionary<int, GameActor> _goModelHash2Actor = new Dictionary<int, GameActor>();
        private List<GameActor> _listActor = new List<GameActor>();
        private GameActor _mainPlayer = null;
        private int _tickRefreshVisible;
        private Dictionary<int, System.Type> _actorTypes = new Dictionary<int, System.Type>();
        public Transform ActorRootTrans { get; set; }

        public void RegisterActorType(int actorType,System.Type type)
        {
            if (!_actorTypes.ContainsKey(actorType))
            {
                _actorTypes.Add(actorType,type);
            }
        }

        private void RegisterAllTypes()
        {
            System.Type baseType = typeof(GameActor);
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Type[] types = assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                if (!types[i].IsClass || types[i].IsAbstract)
                {
                    continue;
                }
                if (types[i].BaseType == baseType)
                {
                    GameActor actor = (GameActor)System.Activator.CreateInstance(types[i]);
                    RegisterActorType(actor.GetActorType(),actor.GetType());
                    actor = null;
                }
            }
        }

        public override void Awake()
        {
            InitActorRoot();

            RegisterAllTypes();

            _tickRefreshVisible = TimerMgr.Instance.AddTimer(o => { RefreshActorVisible(); }, 1f, true, true);
        }

        private void InitActorRoot()
        {
            var actorRoot = GameObject.Find("ActorRoot");
            if (actorRoot == null)
            {
                actorRoot = new GameObject("ActorRoot");
                Object.DontDestroyOnLoad(actorRoot);
                ActorRootTrans = actorRoot.transform;
                ActorRootTrans.position = Vector3.zero;
                ActorRootTrans.rotation = Quaternion.identity;
                ActorRootTrans.localScale = Vector3.one;
            }
        }

        private void RefreshActorVisible()
        {
        }

        public GameObject CreateGameObject(GameActor gameActor)
        {
            string name = gameActor.GetGameObjectName();
            GameObject actorGo = new GameObject(name);
            var trans = actorGo.transform;
            trans.parent = ActorRootTrans;
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
            return actorGo;
        }

        private uint AllocClientGid()
        {
            ++_clientGid;
            if (_clientGid >= ClientGidEnd)
            {
                _clientGid = ClientGidStart;
            }
            return _clientGid;
        }

        public void AddClientActor(GameActor actor)
        {
            if (actor == null)
            {
                return;
            }

            RemoveClientActor(actor);
            actor.ActorId = AllocClientGid();
            _actorPool.Add(actor.ActorId, actor);
            _listActor.Add(actor);
            if (actor.gameObject != null)
            {
                _goModelHash2Actor[actor.gameObject.GetHashCode()] = actor;
            }
        }

        public void RemoveClientActor(GameActor actor)
        {
            if (actor == null)
            {
                return;
            }

            _actorPool.Remove(actor.ActorId);
            _listActor.Remove(actor);
            if (actor.gameObject != null)
            {
                _goModelHash2Actor.Remove(actor.gameObject.GetHashCode());
            }
        }

        public GameActor GetActor(uint actorGID)
        {
            GameActor actor = null;
            _actorPool.TryGetValue(actorGID, out actor);
            return actor;
        }

        public GameActor GetActor(GameObject go)
        {
            GameActor actor;
            if (_goModelHash2Actor.TryGetValue(go.GetHashCode(), out actor))
            {
                return actor;
            }

            return null;
        }

        public void BindGameActorGo(GameActor actor, GameObject go)
        {
            _goModelHash2Actor[go.GetHashCode()] = actor;
        }

        public GameActor GetMainPlayer()
        {
            return _mainPlayer;
        }

        #region Methods

        public bool DestroyActor(GameActor actor)
        {
            return DestroyActor(actor.ActorId);
        }

        public GameActor CreateGameActor(int actorType, uint actorID, bool isMainPlayer)
        {
            GameActor ret = null;

            if (_actorPool.ContainsKey(actorID))
            {
                var oldActor = _actorPool[actorID];
                var oldActorType = oldActor.GetActorType();
                Log.Error("duplicate actor gid {0} {1} {2}", actorID, actorType, oldActorType);
                if (oldActorType != actorType)
                {
                    DestroyActor(actorID);
                    ret = CreateGameActor(actorType, actorID);
                }
                else
                {
                    ret = _actorPool[actorID];
                }
            }
            else
            {
                ret = CreateGameActor(actorType, actorID);
            }

            if (ret == null)
            {
                Log.Error("create actor failed, type is {0}, id is {1}", actorType, actorID);
                return null;
            }

            if (isMainPlayer)
            {
                SetMainPlayer(ret);
            }
            
            ret.Init();

            return ret;
        }

        private GameActor CreateGameActor(int actorType, uint actorID)
        {
            GameActor newActor = null;

            if (_actorTypes.TryGetValue(actorType, out var type))
            {
                newActor = System.Activator.CreateInstance(type) as GameActor;
            }
            else
            {
                Log.Error("unknown actor type:{0}", actorType);
            }

            if (newActor != null)
            {
                newActor.ActorId = actorID;
                _actorPool.Add(actorID, newActor);
                _listActor.Add(newActor);
            }

            return newActor;
        }

        public bool DestroyActor(uint actorID)
        {
            GameActor actor = null;
            if (_actorPool.TryGetValue(actorID, out actor))
            {
                if (actor == _mainPlayer)
                {
                    SetMainPlayer(null);
                }

                if (actor.gameObject != null)
                {
                    _goModelHash2Actor.Remove(actor.gameObject.GetHashCode());
                }

                actor.Destroy();
                _actorPool.Remove(actorID);
                _listActor.Remove(actor);
                return true;
            }

            return false;
        }

        private void SetMainPlayer(GameActor actor)
        {
            _mainPlayer = actor;
        }

        #endregion
    }
}