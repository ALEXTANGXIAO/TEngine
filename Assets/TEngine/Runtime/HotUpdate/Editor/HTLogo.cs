using UnityEngine;
using UnityEditor;

namespace Huatuo.Editor
{
    /// <summary>
    /// 这个类是Huatuo管理器中用到的LOGO图标
    /// </summary>
    public class HTLogo
    {
        private Texture2D _logoImage = null;
        private Rect _rtImage = Rect.zero;

        public int ImgHeight = 0;

        private Material _currentMaterial = null;
        private RippleEffect _rippleEffect = null;

        private EditorWindow _window = null;

        private void InitImage(Vector2 winSize)
        {
            _logoImage = null;
            ImgHeight = 0;
            _rtImage = Rect.zero;

            var files = AssetDatabase.FindAssets("t:texture HuatuoLogoImage");
            if (files == null || files.Length == 0)
            {
                return;
            }

            var file = files[0];
            _logoImage = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(file));
            if (_logoImage == null)
            {
                return;
            }

            ImgHeight = (int) (winSize.x / _logoImage.width * _logoImage.height);
            _rtImage = new Rect(0, 0, winSize.x, ImgHeight);
        }

        private void InitMat()
        {
            if (_logoImage == null)
            {
                return;
            }

            _currentMaterial = null;

            var files = AssetDatabase.FindAssets("t:Material HuatuoLogoMat");
            if (files == null || files.Length == 0)
            {
                return;
            }

            var file = files[0];
            _currentMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(file));
            if (_currentMaterial == null)
            {
                return;
            }

            _rippleEffect = new RippleEffect();
            _rippleEffect.Init(_currentMaterial, _rtImage.width, _rtImage.height);

            EditorApplication.update += Update;
        }

        public void Destroy()
        {
            EditorApplication.update -= Update;

            _rippleEffect?.Destroy();
            _rippleEffect = null;
            _window = null;
        }

        private void Update()
        {
            _rippleEffect?.Update();
            _window?.Repaint();
        }

        public void Init(Vector2 winSize, EditorWindow window)
        {
            _window = window;
            InitImage(winSize);
            //InitMat();
        }

        public void OnGUI()
        {
            if (_logoImage == null)
            {
                return;
            }

            Graphics.DrawTexture(_rtImage, _logoImage, _currentMaterial);
        }
    }
}
