using GameBase;
using UnityEngine;

namespace TEngine
{
    enum ActorShaderGroupType
    {
        /// <summary>
        /// 通用的角色shader
        /// </summary>
        Brdf = 0,

        /// <summary>
        /// 眼睛
        /// </summary>
        BrdfEye,

        ///可能后面扩展，比如特效的特殊角色材质
        GroupMax,
    }

    enum ActorShaderEnvType
    {
        /// <summary>
        /// 游戏内场景默认模型，不带阴影，不带xray，不透明效果
        /// </summary>
        EnvNormal = 0,

        /// <summary>
        /// 展示场景
        /// </summary>
        EnvShow,

        /// <summary>
        /// 带阴影
        /// </summary>
        EnvShadow,

        /// <summary>
        /// 带xray，默认也带Shadow
        /// </summary>
        EnvXRay,

        /// <summary>
        /// 透明渐隐效果
        /// </summary>
        EnvAlphaFade,

        /// <summary>
        /// 展示场景没shadow
        /// </summary>
        EnvShow_NoShadow,

        EnvTypeMax
    }

    /// <summary>
    /// 角色Shader管理器。
    /// </summary>
    class ActorShaderMgr : Singleton<ActorShaderMgr>
    {
        private readonly ActorShaderGroup[] _allShaderGroup = new ActorShaderGroup[(int)ActorShaderGroupType.GroupMax];

        public ActorShaderMgr()
        {
            CreateBrdfShader();
        }

        /// <summary>
        /// 根据当前Render查找角色的Shader分组。
        /// </summary>
        /// <param name="render">Render。</param>
        /// <returns>角色的Shader分组。</returns>
        public ActorShaderGroup FindShaderGroup(Renderer render)
        {
            var sharedMat = render.sharedMaterial;
            if (sharedMat == null)
            {
                return null;
            }

            var shader = sharedMat.shader;
            foreach (var group in _allShaderGroup)
            {
                if (group != null && group.IsMatch(shader))
                {
                    return group;
                }
            }
            return null;
        }

        private void CreateBrdfShader()
        {
            //通用的效果
            var actorShader = new ActorShaderGroup();
            actorShader.AddShader(ActorShaderEnvType.EnvNormal, new TShader("TEngine/Actor/ActorBrdf",shaderLocation:"ActorBrdf"));
            actorShader.AddShader(ActorShaderEnvType.EnvShow, new TShader("TEngine/Actor/Show/ActorBrdf",shaderLocation:"ActorBrdf_Show"));
            actorShader.AddShader(ActorShaderEnvType.EnvShow_NoShadow, new TShader("TEngine/Actor/Show/ActorBrdf_NoShadow",shaderLocation:"ActorBrdf_NoShadow"));
            actorShader.AddShader(ActorShaderEnvType.EnvShadow, new TShader("TEngine/Actor/ActorBrdf",shaderLocation:"ActorBrdf_Normal"));
            actorShader.AddShader(ActorShaderEnvType.EnvXRay, new TShader("TEngine/Actor/X-Ray",shaderLocation:"X-Ray"));
            actorShader.AddShader(ActorShaderEnvType.EnvAlphaFade, new TShader("TEngine/Actor/Fade/ActorBrdf",shaderLocation:"ActorBrdf_Fade"));
            _allShaderGroup[(int)ActorShaderGroupType.Brdf] = actorShader;


            //眼睛效果
            actorShader = new ActorShaderGroup();
            actorShader.AddShader(ActorShaderEnvType.EnvNormal, new TShader("TEngine/Actor/ActorEye",shaderLocation:"ActorEye"));
            actorShader.AddShader(ActorShaderEnvType.EnvShow, new TShader("TEngine/Actor/Show/ActorEye",shaderLocation:"ActorEye_Show", "MRT_DISABLE", "MRT_ENABLE"));
            actorShader.AddShader(ActorShaderEnvType.EnvShadow, new TShader("TEngine/Actor/ActorEye",shaderLocation:"ActorEye"));
            actorShader.AddShader(ActorShaderEnvType.EnvXRay, new TShader("TEngine/Actor/ActorEye",shaderLocation:"ActorEye"));
            actorShader.AddShader(ActorShaderEnvType.EnvAlphaFade, new TShader("TEngine/Actor/Fade/ActorEye",shaderLocation:"ActorEye_Fade"));
            
            _allShaderGroup[(int)ActorShaderGroupType.BrdfEye] = actorShader;
        }
    }
}