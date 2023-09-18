using UnityEngine;

namespace TEngine
{
    public class UIGaussianBlurLayer : MonoBehaviour
    {
        public UnityEngine.UI.RawImage rawImage;
        public Shader shader;
        
        [Range(0, 6), Tooltip("[降采样次数]向下采样的次数。此值越大,则采样间隔越大,需要处理的像素点越少,运行速度越快。")]
        public int DownSampleNum = 2;

        [Range(0.0f, 20.0f), Tooltip("[模糊扩散度]进行高斯模糊时，相邻像素点的间隔。此值越大相邻像素间隔越远，图像越模糊。但过大的值会导致失真。")]
        public float BlurSpreadSize = 3.0f;

        [Range(0, 8), Tooltip("[迭代次数]此值越大,则模糊操作的迭代次数越多，模糊效果越好，但消耗越大。")]
        public int BlurIterations = 3;

        private Camera m_camera;
        private RenderTexture m_renderTexture;
        private Material m_material;
        private string m_shaderName = "UI/UIGaussianBlurLayer";
        private Color m_color;
        public float targetAlpha = 1f;
        private static readonly int DownSampleValue = Shader.PropertyToID("_DownSampleValue");

        #region MaterialGetAndSet

        Material material
        {
            get
            {
                if (m_material == null)
                {
                    m_material = new Material(shader);
                    m_material.hideFlags = HideFlags.HideAndDontSave;
                }

                return m_material;
            }
        }

        #endregion

        void Start()
        {
            m_camera = GetComponent<Camera>();
            if (shader == null)
            {
                shader = Shader.Find(m_shaderName);
                if (shader == null)
                {
                    var tShader = new TShader(m_shaderName);
                    if (tShader.Shader != null)
                    {
                        shader = tShader.Shader;
                    }
                }
            }
            
            m_color = rawImage.color;
            m_color.a = 1f;
        }

        private void Cleanup()
        {
            if (m_material)
            {
                Object.DestroyImmediate(m_material);
            }

            if (rawImage.texture)
            {
                RenderTexture.ReleaseTemporary(m_renderTexture);
            }
        }

        private void OnEnable()
        {
            Cleanup();
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, dest);

            if (!gameObject.activeInHierarchy && enabled)
            {
                return;
            }

            if (!m_camera || !shader || m_renderTexture != null)
            {
                return;
            }

            float widthMod = 1.0f / (1.0f * (1 << DownSampleNum));
            material.SetFloat(DownSampleValue, BlurSpreadSize * widthMod);

            int renderWidth = src.width >> DownSampleNum;
            int renderHeight = src.height >> DownSampleNum;
            m_renderTexture = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, RenderTextureFormat.Default);
            m_renderTexture.filterMode = FilterMode.Bilinear;

            Graphics.Blit(src, m_renderTexture, material, 0);
            for (int i = 0; i < BlurIterations; i++)
            {
                //【2.1】Shader参数赋值
                //迭代偏移量参数
                float iterationOffs = (i * 1.0f);
                //Shader的降采样参数赋值
                material.SetFloat(DownSampleValue, BlurSpreadSize * widthMod + iterationOffs);
                // 【2.2】处理Shader的通道1，垂直方向模糊处理 || Pass1,for vertical blur
                // 定义一个临时渲染的缓存tempBuffer
                RenderTexture tempBuffer = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, RenderTextureFormat.Default);
                // 拷贝rawTexture中的渲染数据到tempBuffer,并仅绘制指定的pass1的纹理数据
                Graphics.Blit(m_renderTexture, tempBuffer, material, 1);
                //  清空rawTexture
                RenderTexture.ReleaseTemporary(m_renderTexture);
                // 将tempBuffer赋给rawTexture，此时rawTexture里面pass0和pass1的数据已经准备好
                m_renderTexture = tempBuffer;
                // 【2.3】处理Shader的通道2，竖直方向模糊处理 || Pass2,for horizontal blur
                // 获取临时渲染纹理
                tempBuffer = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, RenderTextureFormat.Default);
                // 拷贝rawTexture中的渲染数据到tempBuffer,并仅绘制指定的pass2的纹理数据
                Graphics.Blit(m_renderTexture, tempBuffer, m_material, 2);
                //【2.4】得到pass0、pass1和pass2的数据都已经准备好的rawTexture
                // 再次清空rawTexture
                RenderTexture.ReleaseTemporary(m_renderTexture);
                // 再次将tempBuffer赋给rawTexture，此时rawTexture里面pass0、pass1和pass2的数据都已经准备好
                m_renderTexture = tempBuffer;
            }

            rawImage.texture = m_renderTexture;
            m_color.a = targetAlpha;
            rawImage.color = m_color;
            m_camera.enabled = false;
            enabled = false;
        }
    }
}