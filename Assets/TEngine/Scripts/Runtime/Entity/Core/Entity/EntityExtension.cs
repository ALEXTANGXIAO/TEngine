namespace TEngine.Runtime.Entity
{
    public static class EntityExtension
    {
        private static int _serialId = 0;

        /// <summary>
        /// 生成客户端序列化ID，服务器默认不用
        /// </summary>
        /// <param name="entityComponent"></param>
        /// <returns></returns>
        public static int GenerateSerialId(this EntityComponent entityComponent)
        {
            return ++_serialId;
        }

        public static void ShowEntity(this EntityComponent entityComponent, int serialId, int entityId,
            System.Type logicType, string assetPath, object userData = null, float autoReleaseInterval = 60f,
            int capacity = 60,
            float expireTime = 60f, int priority = 0)
        {
            if (serialId < 0)
            {
                Log.Error("Can not load entity id '{0}' from data table.", entityId.ToString());
                return;
            }

            if (!entityComponent.HasEntityGroup(logicType.Name))
            {
                EntityComponent.Instance.AddEntityGroup(logicType.Name,
                    autoReleaseInterval, capacity,
                    expireTime, priority);
            }

            entityComponent.ShowEntity(serialId, logicType, assetPath, logicType.Name,
                Constant.DefaultPriority, userData);
        }
    }
}