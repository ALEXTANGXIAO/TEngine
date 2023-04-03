using YooAsset;

namespace TEngine
{
    public enum UIBaseType
    {
        None,
        Window,
        Widget,
    }
    
    public class UIBase
    {
        /// <summary>
        /// 所属的window。
        /// </summary>
        protected UIBase parent = null;
        
        /// <summary>
        /// UI父节点。
        /// </summary>
        public UIBase Parent => parent;
        
        /// <summary>
        /// UI类型。
        /// </summary>
        public virtual UIBaseType BaseType => UIBaseType.None;
        
        /// <summary>
        /// 资源操作句柄。
        /// </summary>
        public AssetOperationHandle Handle { protected set; get; }
    }
}