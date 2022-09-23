using TEngine.Runtime.Entity;
using UnityEngine;

namespace TEngine.Runtime.Actor
{
    public class PlayEntityMgr:UnitySingleton<PlayEntityMgr>
    {
        protected override void OnLoad()
        {
            RegisterEvent();

            base.OnLoad();
        }

        /// <summary>
        /// RegisterEvent
        /// </summary>
        void RegisterEvent()
        {
            GameEventMgr.Instance.AddEventListener<IEntity, float, object>(EntityEvent.ShowEntitySuccess, OnShowEntitySuccess);
            GameEventMgr.Instance.AddEventListener<int, string, string, string, object>(EntityEvent.ShowEntityFailure, OnShowEntityFailure);
            GameEventMgr.Instance.AddEventListener<int, string, IEntityGroup, object>(EntityEvent.HideEntityComplete, OnHideEntityComplete);
        }

        /// <summary>
        /// OnShowEntitySuccess
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="duration"></param>
        /// <param name="userData"></param>
        private void OnShowEntitySuccess(IEntity entity, float duration, object userData)
        {
            Log.Warning("OnShowEntitySuccess" + entity.ToString() + " " + duration + " " + userData);
        }

        /// <summary>
        /// OnShowEntityFailure
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityAssetName"></param>
        /// <param name="entityGroupName"></param>
        /// <param name="errorMessage"></param>
        /// <param name="userData"></param>
        private void OnShowEntityFailure(int entityId, string entityAssetName, string entityGroupName,
            string errorMessage, object userData)
        {
        }

        /// <summary>
        /// OnHideEntityComplete
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityAssetName"></param>
        /// <param name="entityGroup"></param>
        /// <param name="userData"></param>
        private void OnHideEntityComplete(int entityId, string entityAssetName, IEntityGroup entityGroup,
            object userData)
        {
           
        }

        /// <summary>
        /// 玩家Actor实体管理器创建光源/ URP可最佳实践
        /// </summary>
        /// <param name="lightType"></param>
        /// <param name="position"></param>
        /// <param name="quaternion"></param>
        public void CreatePlayerEntity(int actorId,string entityPath, Vector3 position,Quaternion quaternion)
        {
            EntityData data = EntityData.Create(position,quaternion,actorId);

            EntitySystem.Instance.CreateEntity<ActorEntity>(entityPath, data);
        }
    }
}