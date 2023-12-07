using System.Collections.Generic;
using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// TShader scripts used for all rendering.
    /// <remarks>统一封装对shader的管理。</remarks>
    /// </summary>
    public class TShader
    {
        private bool _loaded;
        private Shader _shader;
        private readonly string _shaderName;
        private readonly string _shaderLocation;
        private readonly List<string> _keywordOn = new List<string>();
        private readonly List<string> _keywordOff = new List<string>();

        /// <summary>
        /// Shader scripts used for all rendering.
        /// </summary>
        public Shader Shader
        {
            get
            {
                if (!_loaded)
                {
                    _loaded = true;
                    _shader = FindShader(_shaderLocation,_shaderName);

                    if (_shader == null)
                    {
                        Log.Error($"invalid shader path: {_shaderLocation}, shader name {_shaderName}");
                    }
                }

                return _shader;
            }
        }
        
        /// <summary>
        /// 查找Shader。
        /// </summary>
        /// <param name="shaderLocation">Shader定位地址。</param>
        /// <param name="shaderName">Shader名称。</param>
        /// <returns>Shader实例。</returns>
        public static Shader FindShader(string shaderLocation,string shaderName)
        {
            Shader shader = GameModule.Resource.LoadAsset<Shader>(shaderLocation);
            if (shader != null)
            {
                return shader;
            }
            return Shader.Find(shaderName);
        }

        /// <summary>
        /// TShader构造函数。
        /// </summary>
        /// <param name="shaderName">shader名称。</param>
        /// <param name="shaderLocation">shader路径。</param>
        public TShader(string shaderName, string shaderLocation)
        {
            _shaderName = shaderName;
            _shaderLocation = shaderLocation;
            _shader = null;
        }

        /// <summary>
        /// TShader构造函数。
        /// </summary>
        /// <param name="shaderName">shader名称。</param>
        /// <param name="shaderLocation">shader路径。</param>
        /// <param name="keywordOn">开启选项。</param>
        /// <param name="keywordOff">关闭选项。</param>
        public TShader(string shaderName, string shaderLocation, string keywordOn, string keywordOff)
        {
            _shaderName = shaderName;
            _shaderLocation = shaderLocation;
            _shader = null;
            _keywordOn.Add(keywordOn);
            _keywordOff.Add(keywordOff);
        }

        /// <summary>
        /// TShader构造函数。
        /// </summary>
        /// <param name="shaderName">shader名称。</param>
        /// <param name="shaderLocation">shader路径。</param>
        /// <param name="keywordOn">开启选项。</param>
        /// <param name="keywordOff">关闭选项。</param>
        public TShader(string shaderName, string shaderLocation, string[] keywordOn, string[] keywordOff)
        {
            _shaderName = shaderName;
            _shaderLocation = shaderLocation;
            _shader = null;
            _keywordOn.AddRange(keywordOn);
            _keywordOff.AddRange(keywordOff);
        }

        /// <summary>
        /// 设置Shader效果。
        /// </summary>
        /// <param name="render">渲染对象。</param>
        public void ApplyRender(Renderer render)
        {
            var sharedMat = render.sharedMaterial;
            if (sharedMat != null)
            {
                //copy一份材质
                sharedMat = render.material;
                sharedMat.shader = Shader;

                foreach (var keyword in _keywordOff)
                {
                    sharedMat.DisableKeyword(keyword);
                }

                foreach (var keyword in _keywordOn)
                {
                    sharedMat.EnableKeyword(keyword);
                }
            }
        }

        /// <summary>
        /// 清除shader。
        /// </summary>
        /// <param name="render">渲染对象。</param>
        public void ClearRender(Renderer render)
        {
            if (_keywordOff.Count <= 0 && _keywordOn.Count <= 0)
            {
                return;
            }

            var sharedMat = render.sharedMaterial;
            if (sharedMat != null)
            {
                //copy一份材质。
                sharedMat = render.material;
                for (int k = 0; k < _keywordOn.Count; k++)
                {
                    sharedMat.DisableKeyword(_keywordOn[k]);
                }

                for (int k = 0; k < _keywordOff.Count; k++)
                {
                    sharedMat.EnableKeyword(_keywordOff[k]);
                }
            }
        }
    }
}