namespace TEngine
{
    enum ActorShaderEnvType
    {
        /// <summary>
        /// 游戏内场景默认模型。
        /// </summary>
        EnvNormal = 0,

        /// <summary>。
        /// 展示场景。
        /// </summary>
        EnvShow,

        /// <summary>
        /// 带阴影。
        /// </summary>
        EnvShadow,

        /// <summary>
        /// 带xray，默认也带Shadow。
        /// </summary>
        EnvXRay,

        /// <summary>
        /// 透明渐隐效果。
        /// </summary>
        EnvAlphaFade,

        EnvTypeMax
    }
        
    enum ActorShaderGroupType
    {
        /// <summary>
        /// 通用的角色shader。
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 眼睛。
        /// </summary>
        NormalEye,

        ///可能后面扩展，比如特效的特殊角色材质。
        GroupMax,
    }
}