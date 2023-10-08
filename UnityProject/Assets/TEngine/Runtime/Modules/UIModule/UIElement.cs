using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// UI元素节点。
    /// <remarks>通过mono序列化来绑定ui节点的元素换取查找与ui的稳定性。</remarks>
    /// </summary>
    public class UIElement : MonoBehaviour
    {
        /// <summary>
        /// UI元素。
        /// </summary>
        [SerializeField] protected SerializableDictionary<string, Transform> elements = new SerializableDictionary<string, Transform>();

        /// <summary>
        /// UI元素。
        /// </summary>
        public SerializableDictionary<string, Transform> Elements => elements;

        /// <summary>
        /// 获取UI元素。
        /// </summary>
        /// <param name="uiID">ui元素标识。</param>
        /// <returns>UI元素。</returns>
        public Transform Get(string uiID)
        {
            elements.TryGetValue(uiID, out var uiTransform);
            return uiTransform;
        }

        /// <summary>
        /// 获取UI元素。
        /// </summary>
        /// <typeparam name="T">ui元素类型。</typeparam>
        /// <param name="uiID">ui元素标识。</param>
        /// <returns>ui元素标识。</returns>
        public T Get<T>(string uiID) where T : Component
        {
            var uiTransform = Get(uiID);
            return uiTransform == null ? null : uiTransform.GetComponent<T>();
        }

        public void OnDestroy()
        {
            elements.Clear();
            elements = null;
        }
    }
}