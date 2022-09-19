using System;
using System.Collections.Generic;

namespace TEngine.Runtime.Actor
{
    public abstract partial class GameActor
    {
        public ActorAttribute Attr = new ActorAttribute();
        private readonly List<ActorComponent> _listComponents = new List<ActorComponent>();
        private readonly Dictionary<string, ActorComponent> _mapComponents = new Dictionary<string, ActorComponent>();
        private bool _isDestroying = false;

        protected void InitExt()
        {
            _isDestroying = false;
        }

        #region component

        public T AddComponent<T>() where T : ActorComponent, new()
        {
            if (IsDestroyed || _isDestroying)
            {
                Log.Fatal("Actor is destroyed, cant add component: {0}, Is Destroying[{1}]",
                    GetClassName(typeof(T)), _isDestroying);
                return null;
            }

            T component = GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            component = ActorComponentPool.Instance.AllocComponent<T>();
            if (!AddComponentImp(component))
            {
                Log.Warning("AddComponent failed, Component name: {0}", GetClassName(typeof(T)));
                component.BeforeDestroy();
                ActorComponentPool.Instance.FreeComponent(component);
                return null;
            }

            return component;
        }

        public T GetComponent<T>() where T : ActorComponent
        {
            ActorComponent component;
            if (_mapComponents.TryGetValue(GetClassName(typeof(T)), out component))
            {
                return component as T;
            }

            return null;
        }

        public void RemoveComponent<T>() where T : ActorComponent
        {
            if (_isDestroying)
            {
                Log.Debug("GameActor[{0}] is destroying, no need destroy component anyway", Name);
                return;
            }

            string className = GetClassName(typeof(T));
            ActorComponent component;
            if (_mapComponents.TryGetValue(className, out component))
            {
                component.BeforeDestroy();

                Event.RemoveAllListenerByOwner(component);
                _mapComponents.Remove(className);
                _listComponents.Remove(component);

                ActorComponentPool.Instance.FreeComponent(component);
            }
        }

        private bool AddComponentImp<T>(T component) where T : ActorComponent
        {
            if (!component.BeforeAddToActor(this))
            {
                return false;
            }

            _listComponents.Add(component);
            _mapComponents[GetClassName(typeof(T))] = component;
            return true;
        }

        private string GetClassName(Type type)
        {
            return type.FullName;
        }

        private void BeforeDestroyAllComponent()
        {
            var listCmpt = _listComponents;
            for (int i = listCmpt.Count - 1; i >= 0; i--)
            {
                listCmpt[i].BeforeDestroy();
            }
        }

        private void DestroyAllComponent()
        {
            var componentPool = ActorComponentPool.Instance;

            var listComponents = _listComponents;
            for (int i = listComponents.Count - 1; i >= 0; i--)
            {
                componentPool.FreeComponent(listComponents[i]);
            }
            _listComponents.Clear();
            _mapComponents.Clear();
        }
        #endregion
    }
}