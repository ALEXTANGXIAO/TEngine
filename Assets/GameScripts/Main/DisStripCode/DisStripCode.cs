using UnityEngine;

/// <summary>
/// 防止裁剪引用。
/// <remarks>如果在主工程无引用，link.xml的防裁剪也无效。</remarks>
/// </summary>
public class DisStripCode : MonoBehaviour
{
   private void Awake()
   {
      //UnityEngine.Physics
      RegisterType<Collider>();
      RegisterType<Collider2D>();
      RegisterType<Collision>();
      RegisterType<Collision2D>();
      
      //UnityEngine.Graphics
      RegisterType<Mesh>();
      RegisterType<MeshRenderer>();
      
      //UnityEngine.Animation
      RegisterType<Animator>();
      RegisterType<Animation>();
   }

   private void RegisterType<T>()
   {
#if UNITY_EDITOR && false
      Debug.Log($"DisStripCode RegisterType :{typeof(T)}");
#endif
   }
}
