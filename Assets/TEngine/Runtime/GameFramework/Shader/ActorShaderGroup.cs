using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 封装一个角色可能用到的各种shader场景
    /// </summary>
    class ActorShaderGroup
    {
        private readonly TShader[] _allShader = new TShader[(int)ActorShaderEnvType.EnvTypeMax];

        public void AddShader(ActorShaderEnvType shaderType, TShader shader)
        {
            _allShader[(int)shaderType] = shader;
        }

        public TShader GetShader(ActorShaderEnvType type)
        {
            return _allShader[(int)type];
        }

        /// <summary>
        /// 判断是否符合shader集合。
        /// </summary>
        /// <param name="shader"></param>
        /// <returns></returns>
        public bool IsMatch(Shader shader)
        {
            for (int i = 0; i < _allShader.Length; i++)
            {
                var dodShader = _allShader[i];
                if (dodShader != null && dodShader.Shader == shader)
                {
                    return true;
                }
            }

            return false;
        }
    }
}