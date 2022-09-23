using System.Collections.Generic;
using TEngine.Runtime;
using TEngine.Runtime.Entity;
using UnityEngine;

namespace TEngine.Demo.TEngine.EntityDemo
{
    /// <summary>
    /// an enumeration of the types of light
    /// </summary>
    public enum LightType
    {
        DirectionalLight = 0,
        PointLight = 1,
        SpotLight = 2,
    }

    /// <summary>
    /// 光源实体管理器
    /// </summary>
    public class LightEntityMgr : UnitySingleton<LightEntityMgr>
    {
        public Dictionary<int, IEntity> m_dictionary = new Dictionary<int, IEntity>();
        public Dictionary<LightType, string> TypeDic = new Dictionary<LightType, string>();

        /// <summary>
        /// OnLoad
        /// </summary>
        protected override void OnLoad()
        {
            TypeDic.Add(LightType.DirectionalLight, "Light/DirectionalLight");
            TypeDic.Add(LightType.PointLight, "Light/PointLight");
            TypeDic.Add(LightType.SpotLight, "Light/SpotLight");
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

            if (!TypeDic.ContainsValue(entity.EntityAssetName))
            {
                return;
            }

            if (!m_dictionary.ContainsKey(entity.Id))
            {
                m_dictionary.Add(entity.Id, entity);
            }
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
            if (!TypeDic.ContainsValue(entityAssetName))
            {
                return;
            }

            if (m_dictionary.ContainsKey(entityId))
            {
                m_dictionary.Remove(entityId);
            }
        }

        /// <summary>
        /// Light实体管理器创建光源/ URP可最佳实践
        /// </summary>
        /// <param name="lightType"></param>
        /// <param name="position"></param>
        /// <param name="quaternion"></param>
        public void CreateLight(LightType lightType, Vector3 position,Quaternion quaternion)
        {
            if (TypeDic.TryGetValue(lightType, out var lightPath))
            {
                EntityData data = EntityData.Create(position,quaternion);
                
                EntitySystem.Instance.CreateEntity<LightEntity>(lightPath, data);
            }
        }
    }
}