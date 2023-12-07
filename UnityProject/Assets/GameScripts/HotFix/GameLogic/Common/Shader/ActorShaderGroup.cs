using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 封装一个角色可能用到的各种shader场景。
    /// </summary>
    class ActorShaderGroup
    {
        private readonly TShader[] _allShader = new TShader[(int)ActorShaderEnvType.EnvTypeMax];

        /// <summary>
        /// 增加Shader到角色Shader分组。
        /// </summary>
        /// <param name="shaderType">当前环境类型。</param>
        /// <param name="shader">TShader。</param>
        public void AddShader(ActorShaderEnvType shaderType, TShader shader)
        {
            _allShader[(int)shaderType] = shader;
        }

        /// <summary>
        /// 根据当前环境获取Shader。
        /// </summary>
        /// <param name="type">当前环境类型。</param>
        /// <returns>TShader。</returns>
        public TShader GetShader(ActorShaderEnvType type)
        {
            return _allShader[(int)type];
        }

        /// <summary>
        /// 判断是否符合shader集合。
        /// </summary>
        /// <param name="shader">Shader实例。</param>
        /// <returns>是否符合。</returns>
        public bool IsMatch(Shader shader)
        {
            foreach (var dodShader in _allShader)
            {
                if (dodShader != null && dodShader.Shader == shader)
                {
                    return true;
                }
            }
            return false;
        }
    }
}