using System;
using System.Collections.Generic;
using UnityEngine;

namespace TEngine
{
    #region Architecture

    abstract class Architecture<T> : ILogicSys, IArchitecture where T : Architecture<T>, new()
    {
        private bool _init;

        private readonly List<ISystem> _systems = new List<ISystem>();

        private readonly List<IModel> _models = new List<IModel>();

        public static readonly Action<T> OnRegisterPatch = architecture => { };

        private static T _architecture;

        #region AttributeList

        private List<SubSystem> _updateSystems;
        private List<SubSystem> _lateUpdateSystems;
        private List<SubSystem> _fixedUpdateSystems;
        private List<SubSystem> _roleLoginSystems;
        private List<SubSystem> _roleLogoutSystems;

        #endregion

        public static IArchitecture Interface
        {
            get
            {
                if (_architecture == null)
                {
                    MakeSureArchitecture();
                }

                return _architecture;
            }
        }

        public static T Instance
        {
            get
            {
                if (_architecture == null)
                {
                    MakeSureArchitecture();
                }

                return _architecture;
            }
        }

        static void MakeSureArchitecture()
        {
            if (_architecture == null)
            {
                _architecture = new T();
                _architecture.Init();

                if (OnRegisterPatch != null)
                {
                    OnRegisterPatch.Invoke(_architecture);
                }

                foreach (var architectureModel in _architecture._models)
                {
                    architectureModel.Init();
                }

                _architecture._models.Clear();

                foreach (var architectureSystem in _architecture._systems)
                {
                    architectureSystem.Init();
                }

                _architecture._systems.Clear();

                _architecture._init = true;
            }
        }

        protected abstract void Init();

        private readonly Container _container = new Container();

        public void RegisterSystem<TSystem>(TSystem system) where TSystem : ISystem
        {
            system.SetArchitecture(this);
            _container.Register(system);

            if (!_init)
            {
                ProcessAttribute(system);
                _systems.Add(system);
            }
            else
            {
                system.Init();
            }
        }

        public void ProcessAttribute<TSystem>(TSystem system) where TSystem : ISystem
        {
            Type systemType = system.GetType();
            SubSystem instance = system as SubSystem;
            if (HadAttribute<UpdateAttribute>(systemType))
            {
                if (_updateSystems == null)
                {
                    _updateSystems = new List<SubSystem>();
                }

                _updateSystems.Add(instance);
            }

            if (HadAttribute<LateUpdateAttribute>(systemType))
            {
                if (_lateUpdateSystems == null)
                {
                    _lateUpdateSystems = new List<SubSystem>();
                }

                _lateUpdateSystems.Add(instance);
            }

            if (HadAttribute<FixedUpdateAttribute>(systemType))
            {
                if (_fixedUpdateSystems == null)
                {
                    _fixedUpdateSystems = new List<SubSystem>();
                }

                _fixedUpdateSystems.Add(instance);
            }

            if (HadAttribute<RoleLoginAttribute>(systemType))
            {
                if (_roleLoginSystems == null)
                {
                    _roleLoginSystems = new List<SubSystem>();
                }

                _roleLoginSystems.Add(instance);
            }

            if (HadAttribute<RoleLogoutAttribute>(systemType))
            {
                if (_roleLogoutSystems == null)
                {
                    _roleLogoutSystems = new List<SubSystem>();
                }

                _roleLogoutSystems.Add(instance);
            }
        }

        private bool HadAttribute<TAttribute>(Type type) where TAttribute : Attribute
        {
            TAttribute attribute = Attribute.GetCustomAttribute(type, typeof(TAttribute)) as TAttribute;

            return attribute != null;
        }

        public void RegisterModel<TModel>(TModel model) where TModel : IModel
        {
            model.SetArchitecture(this);
            _container.Register(model);

            if (!_init)
            {
                _models.Add(model);
            }
            else
            {
                model.Init();
            }
        }

        public void RegisterUtility<TUtility>(TUtility utility) where TUtility : IUtility
        {
            _container.Register(utility);
        }

        public TSystem GetSystem<TSystem>() where TSystem : class, ISystem
        {
            return _container.Get<TSystem>();
        }

        public TModel GetModel<TModel>() where TModel : class, IModel
        {
            return _container.Get<TModel>();
        }

        public TUtility GetUtility<TUtility>() where TUtility : class, IUtility
        {
            return _container.Get<TUtility>();
        }

        public void SendCommand<TCommand>() where TCommand : ICommand, new()
        {
            var command = new TCommand();
            command.SetArchitecture(this);
            command.Execute();
        }

        public void SendCommand<TCommand>(TCommand command) where TCommand : ICommand
        {
            command.SetArchitecture(this);
            command.Execute();
        }

        public TResult SendQuery<TResult>(IQuery<TResult> query)
        {
            query.SetArchitecture(this);
            return query.Do();
        }

        private readonly TypeEventSystem _typeEventSystem = new TypeEventSystem();

        public void SendEvent<TEvent>() where TEvent : new()
        {
            _typeEventSystem.Send<TEvent>();
        }

        public void SendEvent<TEvent>(TEvent e)
        {
            _typeEventSystem.Send(e);
        }

        public IUnRegister RegisterEvent<TEvent>(Action<TEvent> onEvent)
        {
            return _typeEventSystem.Register(onEvent);
        }

        public void UnRegisterEvent<TEvent>(Action<TEvent> onEvent)
        {
            _typeEventSystem.UnRegister(onEvent);
        }

        #region 生命周期

        public virtual void OnActive()
        {
        }

        public virtual bool OnInit()
        {
            return true;
        }

        public virtual void OnDestroy()
        {
        }

        public virtual void OnStart()
        {
        }

        public virtual void OnUpdate()
        {
            if (_updateSystems != null)
            {
                foreach (var system in _updateSystems)
                {
                    system.OnUpdate();
                }
            }
        }

        public virtual void OnLateUpdate()
        {
            if (_lateUpdateSystems != null)
            {
                foreach (var system in _lateUpdateSystems)
                {
                    system.OnLateUpdate();
                }
            }
        }

        public virtual void OnFixedUpdate()
        {
            if (_fixedUpdateSystems != null)
            {
                foreach (var system in _fixedUpdateSystems)
                {
                    system.OnLateUpdate();
                }
            }
        }

        public virtual void OnRoleLogin()
        {
            if (_roleLoginSystems != null)
            {
                foreach (var system in _roleLoginSystems)
                {
                    system.OnRoleLogin();
                }
            }
        }

        public virtual void OnRoleLogout()
        {
            if (_roleLogoutSystems != null)
            {
                foreach (var system in _roleLogoutSystems)
                {
                    system.OnRoleLogout();
                }
            }
        }

        public virtual void OnDrawGizmos()
        {
        }

        public virtual void OnApplicationPause(bool pause)
        {
        }

        #endregion
    }

    public interface IArchitecture
    {
        void RegisterSystem<T>(T system) where T : ISystem;

        void RegisterModel<T>(T model) where T : IModel;

        void RegisterUtility<T>(T utility) where T : IUtility;

        T GetSystem<T>() where T : class, ISystem;

        T GetModel<T>() where T : class, IModel;

        T GetUtility<T>() where T : class, IUtility;

        void SendCommand<T>() where T : ICommand, new();

        void SendCommand<T>(T command) where T : ICommand;

        TResult SendQuery<TResult>(IQuery<TResult> query);

        void SendEvent<T>() where T : new();

        void SendEvent<T>(T e);

        IUnRegister RegisterEvent<T>(Action<T> onEvent);

        void UnRegisterEvent<T>(Action<T> onEvent);
    }

    public interface IOnEvent<in T>
    {
        void OnEvent(T e);
    }

    public static class OnGlobalEventExtension
    {
        public static IUnRegister RegisterEvent<T>(this IOnEvent<T> self) where T : struct
        {
            return TypeEventSystem.Global.Register<T>(self.OnEvent);
        }

        public static void UnRegisterEvent<T>(this IOnEvent<T> self) where T : struct
        {
            TypeEventSystem.Global.UnRegister<T>(self.OnEvent);
        }
    }

    #endregion

    #region Controller

    public interface IController : IBelongToArchitecture, ICanSendCommand, ICanGetSystem, ICanGetModel,
        ICanRegisterEvent, ICanSendQuery
    {
    }

    #endregion

    #region System

    public interface ISystem : IBelongToArchitecture, ICanSetArchitecture, ICanGetModel, ICanGetUtility,
        ICanRegisterEvent, ICanSendEvent, ICanGetSystem
    {
        void Init();
    }

    abstract class SubSystem : ISystem, ILogicSys
    {
        private IArchitecture _architecture;

        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return _architecture;
        }

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture)
        {
            _architecture = architecture;
        }

        void ISystem.Init()
        {
            OnInit();
        }

        protected abstract void OnInit();

        public virtual void OnDestroy()
        {
        }

        public virtual void OnStart()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnLateUpdate()
        {
        }

        public virtual void OnFixedUpdate()
        {
        }

        public virtual void OnRoleLogin()
        {
        }

        public virtual void OnRoleLogout()
        {
        }

        public virtual void OnDrawGizmos()
        {
        }

        public virtual void OnApplicationPause(bool pause)
        {
        }

        bool ILogicSys.OnInit()
        {
            return true;
        }
    }

    #endregion

    #region Model

    public interface IModel : IBelongToArchitecture, ICanSetArchitecture, ICanGetUtility, ICanSendEvent
    {
        void Init();
    }

    public abstract class AbstractModel : IModel
    {
        private IArchitecture _architecture;

        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return _architecture;
        }

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture)
        {
            _architecture = architecture;
        }

        void IModel.Init()
        {
            OnInit();
        }

        protected abstract void OnInit();
    }

    #endregion

    #region Utility

    public interface IUtility
    {
    }

    #endregion

    #region Command

    public interface ICommand : IBelongToArchitecture, ICanSetArchitecture, ICanGetSystem, ICanGetModel, ICanGetUtility,
        ICanSendEvent, ICanSendCommand, ICanSendQuery
    {
        void Execute();
    }

    public abstract class AbstractCommand : ICommand
    {
        private IArchitecture _architecture;

        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return _architecture;
        }

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture)
        {
            _architecture = architecture;
        }

        void ICommand.Execute()
        {
            OnExecute();
        }

        protected abstract void OnExecute();
    }

    #endregion

    #region Query

    public interface IQuery<out TResult> : IBelongToArchitecture, ICanSetArchitecture, ICanGetModel, ICanGetSystem,
        ICanSendQuery
    {
        TResult Do();
    }

    public abstract class AbstractQuery<T> : IQuery<T>
    {
        public T Do()
        {
            return OnDo();
        }

        protected abstract T OnDo();


        private IArchitecture _architecture;

        public IArchitecture GetArchitecture()
        {
            return _architecture;
        }

        public void SetArchitecture(IArchitecture architecture)
        {
            _architecture = architecture;
        }
    }

    #endregion

    #region Rule

    public interface IBelongToArchitecture
    {
        IArchitecture GetArchitecture();
    }

    public interface ICanSetArchitecture
    {
        void SetArchitecture(IArchitecture architecture);
    }

    public interface ICanGetModel : IBelongToArchitecture
    {
    }

    public static class CanGetModelExtension
    {
        public static T GetModel<T>(this ICanGetModel self) where T : class, IModel
        {
            return self.GetArchitecture().GetModel<T>();
        }
    }

    public interface ICanGetSystem : IBelongToArchitecture
    {
    }

    public static class CanGetSystemExtension
    {
        public static T GetSystem<T>(this ICanGetSystem self) where T : class, ISystem
        {
            return self.GetArchitecture().GetSystem<T>();
        }
    }

    public interface ICanGetUtility : IBelongToArchitecture
    {
    }

    public static class CanGetUtilityExtension
    {
        public static T GetUtility<T>(this ICanGetUtility self) where T : class, IUtility
        {
            return self.GetArchitecture().GetUtility<T>();
        }
    }

    public interface ICanRegisterEvent : IBelongToArchitecture
    {
    }

    public static class CanRegisterEventExtension
    {
        public static IUnRegister RegisterEvent<T>(this ICanRegisterEvent self, Action<T> onEvent)
        {
            return self.GetArchitecture().RegisterEvent(onEvent);
        }

        public static void UnRegisterEvent<T>(this ICanRegisterEvent self, Action<T> onEvent)
        {
            self.GetArchitecture().UnRegisterEvent(onEvent);
        }
    }

    public interface ICanSendCommand : IBelongToArchitecture
    {
    }

    public static class CanSendCommandExtension
    {
        public static void SendCommand<T>(this ICanSendCommand self) where T : ICommand, new()
        {
            self.GetArchitecture().SendCommand<T>();
        }

        public static void SendCommand<T>(this ICanSendCommand self, T command) where T : ICommand
        {
            self.GetArchitecture().SendCommand(command);
        }
    }

    public interface ICanSendEvent : IBelongToArchitecture
    {
    }

    public static class CanSendEventExtension
    {
        public static void SendEvent<T>(this ICanSendEvent self) where T : new()
        {
            self.GetArchitecture().SendEvent<T>();
        }

        public static void SendEvent<T>(this ICanSendEvent self, T e)
        {
            self.GetArchitecture().SendEvent(e);
        }
    }

    public interface ICanSendQuery : IBelongToArchitecture
    {
    }

    public static class CanSendQueryExtension
    {
        public static TResult SendQuery<TResult>(this ICanSendQuery self, IQuery<TResult> query)
        {
            return self.GetArchitecture().SendQuery(query);
        }
    }

    #endregion

    #region TypeEventSystem

    public interface IUnRegister
    {
        void UnRegister();
    }

    public interface IUnRegisterList
    {
        List<IUnRegister> UnregisterList { get; }
    }

    public static class UnRegisterListExtension
    {
        public static void AddToUnregisterList(this IUnRegister self, IUnRegisterList unRegisterList)
        {
            unRegisterList.UnregisterList.Add(self);
        }

        public static void UnRegisterAll(this IUnRegisterList self)
        {
            foreach (var unRegister in self.UnregisterList)
            {
                unRegister.UnRegister();
            }

            self.UnregisterList.Clear();
        }
    }

    /// <summary>
    /// 自定义可注销的类
    /// </summary>
    public struct CustomUnRegister : IUnRegister
    {
        /// <summary>
        /// 委托对象
        /// </summary>
        private Action _onUnRegister;

        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="onUnRegister"></param>
        public CustomUnRegister(Action onUnRegister)
        {
            _onUnRegister = onUnRegister;
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void UnRegister()
        {
            _onUnRegister.Invoke();
            _onUnRegister = null;
        }
    }

    public class UnRegisterOnDestroyTrigger : MonoBehaviour
    {
        private readonly HashSet<IUnRegister> _unRegisters = new HashSet<IUnRegister>();

        public void AddUnRegister(IUnRegister unRegister)
        {
            _unRegisters.Add(unRegister);
        }

        public void RemoveUnRegister(IUnRegister unRegister)
        {
            _unRegisters.Remove(unRegister);
        }

        private void OnDestroy()
        {
            foreach (var unRegister in _unRegisters)
            {
                unRegister.UnRegister();
            }

            _unRegisters.Clear();
        }
    }

    public static class UnRegisterExtension
    {
        public static IUnRegister UnRegisterWhenGameObjectDestroyed(this IUnRegister unRegister, GameObject gameObject)
        {
            var trigger = gameObject.GetComponent<UnRegisterOnDestroyTrigger>();

            if (!trigger)
            {
                trigger = gameObject.AddComponent<UnRegisterOnDestroyTrigger>();
            }

            trigger.AddUnRegister(unRegister);

            return unRegister;
        }
    }

    public class TypeEventSystem
    {
        private readonly EasyEvents _events = new EasyEvents();


        public static readonly TypeEventSystem Global = new TypeEventSystem();

        public void Send<T>() where T : new()
        {
            var eventInfo = _events.GetEvent<EasyEvent<T>>();
            if (eventInfo != null)
            {
                eventInfo.Trigger(new T());
            }
        }

        public void Send<T>(T e)
        {
            var eventInfo = _events.GetEvent<EasyEvent<T>>();
            if (eventInfo != null)
            {
                eventInfo.Trigger(e);
            }
        }

        public IUnRegister Register<T>(Action<T> onEvent)
        {
            var e = _events.GetOrAddEvent<EasyEvent<T>>();
            return e.Register(onEvent);
        }

        public void UnRegister<T>(Action<T> onEvent)
        {
            var e = _events.GetEvent<EasyEvent<T>>();
            if (e != null)
            {
                e.UnRegister(onEvent);
            }
        }
    }

    #endregion

    #region Redux

    public class Container
    {
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        public void Register<T>(T instance)
        {
            var key = typeof(T);

            if (_instances.ContainsKey(key))
            {
                _instances[key] = instance;
            }
            else
            {
                _instances.Add(key, instance);
            }
        }

        public T Get<T>() where T : class
        {
            var key = typeof(T);

            object ret;
            if (_instances.TryGetValue(key, out ret))
            {
                return ret as T;
            }

            return null;
        }
    }

    #endregion

    #region BindableProperty

    public interface IBindableProperty<T> : IReadonlyBindableProperty<T>
    {
        new T Value { get; set; }
        void SetValueWithoutEvent(T newValue);
    }

    public interface IReadonlyBindableProperty<T>
    {
        T Value { get; }

        IUnRegister RegisterWithInitValue(Action<T> action);
        void UnRegister(Action<T> onValueChanged);
        IUnRegister Register(Action<T> onValueChanged);
    }

    public abstract class BindAbleProperty<T> : IBindableProperty<T>
    {
        public BindAbleProperty(T defaultValue = default(T))
        {
            _value = defaultValue;
        }

        protected T _value;

        public T Value
        {
            get { return GetValue(); }
            set
            {
                if (value == null && _value == null) return;
                if (value != null && value.Equals(_value)) return;

                SetValue(value);
                if (_onValueChanged != null)
                {
                    _onValueChanged.Invoke(value);
                }
            }
        }

        protected virtual void SetValue(T newValue)
        {
            _value = newValue;
        }

        protected virtual T GetValue()
        {
            return _value;
        }

        public void SetValueWithoutEvent(T newValue)
        {
            _value = newValue;
        }

        private Action<T> _onValueChanged = v => { };

        public IUnRegister Register(Action<T> onValueChanged)
        {
            _onValueChanged += onValueChanged;
            return new BindablePropertyUnRegister<T>()
            {
                BindAbleProperty = this,
                OnValueChanged = onValueChanged
            };
        }

        public IUnRegister RegisterWithInitValue(Action<T> onValueChanged)
        {
            onValueChanged(_value);
            return Register(onValueChanged);
        }

        public static implicit operator T(BindAbleProperty<T> property)
        {
            return property.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public void UnRegister(Action<T> onValueChanged)
        {
            _onValueChanged -= onValueChanged;
        }
    }

    public class BindablePropertyUnRegister<T> : IUnRegister
    {
        public BindAbleProperty<T> BindAbleProperty { get; set; }

        public Action<T> OnValueChanged { get; set; }

        public void UnRegister()
        {
            BindAbleProperty.UnRegister(OnValueChanged);

            BindAbleProperty = null;
            OnValueChanged = null;
        }
    }

    #endregion

    #region EasyEvent

    public interface IEasyEvent
    {
    }

    public class EasyEvent : IEasyEvent
    {
        private Action _onEvent = () => { };

        public IUnRegister Register(Action onEvent)
        {
            _onEvent += onEvent;
            return new CustomUnRegister(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action onEvent)
        {
            _onEvent -= onEvent;
        }

        public void Trigger()
        {
            if (_onEvent != null)
            {
                _onEvent.Invoke();
            }
        }
    }

    public class EasyEvent<T> : IEasyEvent
    {
        private Action<T> _onEvent = e => { };

        public IUnRegister Register(Action<T> onEvent)
        {
            _onEvent += onEvent;
            return new CustomUnRegister(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action<T> onEvent)
        {
            _onEvent -= onEvent;
        }

        public void Trigger(T t)
        {
            if (_onEvent != null)
            {
                _onEvent.Invoke(t);
            }
        }
    }

    public class EasyEvent<T, K> : IEasyEvent
    {
        private Action<T, K> _onEvent = (t, k) => { };

        public IUnRegister Register(Action<T, K> onEvent)
        {
            _onEvent += onEvent;
            return new CustomUnRegister(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action<T, K> onEvent)
        {
            _onEvent -= onEvent;
        }

        public void Trigger(T t, K k)
        {
            if (_onEvent != null)
            {
                _onEvent.Invoke(t, k);
            }
        }
    }

    public class EasyEvent<T, K, S> : IEasyEvent
    {
        private Action<T, K, S> _onEvent = (t, k, s) => { };

        public IUnRegister Register(Action<T, K, S> onEvent)
        {
            _onEvent += onEvent;
            return new CustomUnRegister(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action<T, K, S> onEvent)
        {
            _onEvent -= onEvent;
        }

        public void Trigger(T t, K k, S s)
        {
            if (_onEvent != null)
            {
                _onEvent.Invoke(t, k, s);
            }
        }
    }

    public class EasyEvents
    {
        private static readonly EasyEvents GlobalEvents = new EasyEvents();

        public static T Get<T>() where T : IEasyEvent
        {
            return GlobalEvents.GetEvent<T>();
        }


        public static void Register<T>() where T : IEasyEvent, new()
        {
            GlobalEvents.AddEvent<T>();
        }

        private readonly Dictionary<Type, IEasyEvent> _typeEvents = new Dictionary<Type, IEasyEvent>();

        public void AddEvent<T>() where T : IEasyEvent, new()
        {
            _typeEvents.Add(typeof(T), new T());
        }

        public T GetEvent<T>() where T : IEasyEvent
        {
            IEasyEvent e;

            if (_typeEvents.TryGetValue(typeof(T), out e))
            {
                return (T)e;
            }

            return default(T);
        }

        public T GetOrAddEvent<T>() where T : IEasyEvent, new()
        {
            var eType = typeof(T);
            IEasyEvent e;
            if (_typeEvents.TryGetValue(eType, out e))
            {
                return (T)e;
            }

            var t = new T();
            _typeEvents.Add(eType, t);
            return t;
        }
    }

    #endregion
}