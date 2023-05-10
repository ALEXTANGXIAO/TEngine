using System.Collections.Generic;
using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// TShader scripts used for all rendering.
    /// </summary>
    class TShader
    {
        private bool _loaded;
        private Shader _shader;
        private readonly string _shaderPath;
        private readonly List<string> _keywordOn = new();
        private readonly List<string> _keywordOff = new();

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
                    _shader = FindShader(_shaderPath);

                    if (_shader == null)
                    {
                        Debug.LogErrorFormat("invalid shader path: {0}", _shaderPath);
                    }
                }

                return _shader;
            }
        }
        
        /// <summary>
        /// 查找Shader。
        /// </summary>
        /// <param name="shaderName">Shader名字。</param>
        /// <returns>Shader实例。</returns>
        public static Shader FindShader(string shaderName)
        {
            Shader shader = GameModule.Resource.LoadAsset<Shader>(shaderName);
            if (shader != null)
            {
                return shader;
            }
            return Shader.Find(shaderName);
        }

        /// <summary>
        /// TShader构造函数。
        /// </summary>
        /// <param name="shaderPath">shader路径。</param>
        public TShader(string shaderPath)
        {
            _shaderPath = shaderPath;
            _shader = null;
        }

        /// <summary>
        /// TShader构造函数。
        /// </summary>
        /// <param name="shaderPath">shader路径。</param>
        /// <param name="keywordOn">开启选项。</param>
        /// <param name="keywordOff">关闭选项。</param>
        public TShader(string shaderPath, string keywordOn, string keywordOff)
        {
            _shaderPath = shaderPath;
            _shader = null;
            _keywordOn.Add(keywordOn);
            _keywordOff.Add(keywordOff);
        }

        /// <summary>
        /// TShader构造函数。
        /// </summary>
        /// <param name="shaderPath">shader路径。</param>
        /// <param name="keywordOn">开启选项。</param>
        /// <param name="keywordOff">关闭选项。</param>
        public TShader(string shaderPath, string[] keywordOn, string[] keywordOff)
        {
            _shaderPath = shaderPath;
            _shader = null;
            _keywordOn.AddRange(keywordOn);
            _keywordOff.AddRange(keywordOff);
        }

        /// <summary>
        /// 设置Shader效果。
        /// </summary>
        /// <param name="render"></param>
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
        /// <param name="render"></param>
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