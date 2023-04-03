namespace TEngine
{
    public abstract class UIWidget:UIBase,IUIBehaviour
    {
        /// <summary>
        /// 所属的窗口。
        /// </summary>
        public UIWindow OwnerWindow
        {
            get
            {
                var parentUI = base.parent;
                while (parentUI != null)
                {
                    if (parentUI.BaseType == UIBaseType.Window)
                    {
                        return parentUI as UIWindow;
                    }

                    parentUI = parentUI.Parent;
                }

                return null;
            }
        }
        
        public virtual void ScriptGenerator()
        {
            throw new System.NotImplementedException();
        }

        public virtual void BindMemberProperty()
        {
            throw new System.NotImplementedException();
        }

        public virtual void RegisterEvent()
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnCreate()
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnRefresh()
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnUpdate()
        {
            throw new System.NotImplementedException();
        }

        public virtual void OnDestroy()
        {
            throw new System.NotImplementedException();
        }
    }
}