using UnityEngine;

namespace TEngine
{
    /// <summary>
    /// 统一管理ActorShader。
    /// </summary>
    class ActorShaderMgr
    {
        private static ActorShaderMgr _instance;

        public static ActorShaderMgr Instance => _instance ??= new ActorShaderMgr();

        private readonly ActorShaderGroup[] _allShaderGroup = new ActorShaderGroup[(int)ActorShaderGroupType.GroupMax];

        public ActorShaderMgr()
        {
            RegisterShaderGroup();
        }

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

        void RegisterShaderGroup()
        {
            //通用的效果
            var actorShader = new ActorShaderGroup();
            actorShader.AddShader(ActorShaderEnvType.EnvNormal, new TShader("TEngine/Actor/ActorNormal"));
            actorShader.AddShader(ActorShaderEnvType.EnvShow, new TShader("TEngine/Actor/Show/ActorNormal"));

            actorShader.AddShader(ActorShaderEnvType.EnvShadow, new TShader("TEngine/Actor/ActorNormal"));
            actorShader.AddShader(ActorShaderEnvType.EnvXRay, new TShader("TEngine/Actor/X-Ray"));
            actorShader.AddShader(ActorShaderEnvType.EnvAlphaFade, new TShader("TEngine/Actor/Fade/ActorNormal"));
            _allShaderGroup[(int)ActorShaderGroupType.Normal] = actorShader;

            //眼睛效果
            actorShader = new ActorShaderGroup();
            actorShader.AddShader(ActorShaderEnvType.EnvNormal, new TShader("TEngine/Actor/ActorEye"));
            actorShader.AddShader(ActorShaderEnvType.EnvShow, new TShader("TEngine/Actor/Show/ActorEye", "MRT_DISABLE", "MRT_ENABLE"));
            actorShader.AddShader(ActorShaderEnvType.EnvShadow, new TShader("TEngine/Actor/ActorEye"));
            actorShader.AddShader(ActorShaderEnvType.EnvXRay, new TShader("TEngine/Actor/ActorEye"));
            actorShader.AddShader(ActorShaderEnvType.EnvAlphaFade, new TShader("TEngine/Actor/Fade/ActorEye"));

            _allShaderGroup[(int)ActorShaderGroupType.NormalEye] = actorShader;
        }
    }
}