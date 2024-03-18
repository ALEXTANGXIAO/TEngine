using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace TEngine
{
    [Serializable]
    public class SetSpriteObject : ISetAssetObject
    {
        enum SetType
        {
            None,
            Image,
            SpriteRender,
        }
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private SetType setType;
        
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private Image m_Image;
        
#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private SpriteRenderer m_SpriteRenderer;

#if ODIN_INSPECTOR
        [ShowInInspector]
#endif
        private Sprite Sprite;

        public string Location { get; private set; }

        private bool m_SetNativeSize = false;

        public void SetAsset(Object asset)
        {
            Sprite = (Sprite)asset;

            if (m_Image != null)
            {
                m_Image.sprite = Sprite;
                if (m_SetNativeSize)
                {
                    m_Image.SetNativeSize();
                }
            }
            else if (m_SpriteRenderer != null)
            {
                m_SpriteRenderer.sprite = Sprite;
            }
        }

        public bool IsCanRelease()
        {
            if (setType == SetType.Image)
            {
                return m_Image == null || m_Image.sprite == null ||
                       (Sprite != null && m_Image.sprite != Sprite);
            }
            else if (setType == SetType.SpriteRender)
            {
                return m_SpriteRenderer == null || m_SpriteRenderer.sprite == null ||
                       (Sprite != null && m_SpriteRenderer.sprite != Sprite);
            }
            return true;
        }

        public void Clear()
        {
            m_SpriteRenderer = null;
            m_Image = null;
            Location = null;
            Sprite = null;
            setType = SetType.None;
            m_SetNativeSize = false;
        }

        public static SetSpriteObject Create(Image image, string location, bool setNativeSize = false)
        {
            SetSpriteObject item = MemoryPool.Acquire<SetSpriteObject>();
            item.m_Image = image;
            item.m_SetNativeSize = setNativeSize;
            item.Location = location;
            item.setType = SetType.Image;
            return item;
        }
        
        public static SetSpriteObject Create(SpriteRenderer spriteRenderer, string location)
        {
            SetSpriteObject item = MemoryPool.Acquire<SetSpriteObject>();
            item.m_SpriteRenderer = spriteRenderer;
            item.Location = location;
            item.setType = SetType.SpriteRender;
            return item;
        }
    }
}