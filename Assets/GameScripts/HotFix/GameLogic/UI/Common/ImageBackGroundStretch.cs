using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 背景图片等比拉伸
    /// </summary>
    public class ImageBackGroundStretch : MonoBehaviour
    {
        protected virtual void Start()
        {
            DoImageStretch(9/16f);
        }

        private void DoImageStretch(float standardAspect)
        {
            float deviceAspect = Screen.width / (float)Screen.height;
            if (standardAspect > deviceAspect)
            {
                float scale = standardAspect / deviceAspect;
                transform.localScale = new Vector3(scale, scale, 1f);
            }
            else if (standardAspect < deviceAspect)
            {
                float scale = deviceAspect / standardAspect;
                transform.localScale = new Vector3(scale, scale, 1f);
            }
        }
    }
}
