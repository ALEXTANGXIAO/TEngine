using System;
using System.Collections.Generic;

#pragma warning disable CS8625
namespace TEngine
{
    public sealed class FCancellationToken
    {
        private HashSet<Action> _actions = new HashSet<Action>();
        public bool IsCancel => _actions == null;
    
        public void Add(Action action)
        {
            _actions.Add(action);
        }
    
        public void Remove(Action action)
        {
            _actions.Remove(action);
        }
    
        public void Cancel()
        {
            if (_actions == null)
            {
                return;
            }
            
            var runActions = _actions;
            _actions = null;
            
            foreach (var action in runActions)
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
        }
    }
}

