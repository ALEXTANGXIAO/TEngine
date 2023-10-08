using System.Collections.Generic;
using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 内置资源清单。
    /// <remarks>底包内的资源清单。</remarks>
    /// </summary>
    public class BuiltinFileManifest : ScriptableObject
    {
        /// <summary>
        /// 内置游戏版本。
        /// </summary>
        public string internalGameVersion = string.Empty;
        
        /// <summary>
        /// 内置资源清单列表。
        /// </summary>
        public List<string> builtinFiles = new List<string>();
    }
}