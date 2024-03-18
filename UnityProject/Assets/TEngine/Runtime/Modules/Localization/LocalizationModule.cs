using UnityEngine;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using TEngine.Localization;

namespace TEngine
{
    /// <summary>
    /// 本地化组件。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LocalizationModule: Module, IResourceManager_Bundles
    {
        private string m_DefaultLanguage = "Chinese";

        [SerializeField] private TextAsset m_InnerLocalizationCSV;

        private LanguageSource m_LanguageSource;

        private LanguageSourceData m_SourceData
        {
            get
            {
                if (m_LanguageSource == null)
                {
                    m_LanguageSource = gameObject.AddComponent<LanguageSource>();
                }

                return m_LanguageSource.SourceData;
            }
        }

        [SerializeField] private List<string> m_AllLanguage = new List<string>();

        /// <summary>
        /// 模拟平台运行时 编辑器资源不加载。
        /// </summary>
        [SerializeField] private bool m_UseRuntimeModule = true;

        private string m_CurrentLanguage;

        /// <summary>
        /// 获取或设置本地化语言。
        /// </summary>
        public Language Language
        {
            get
            {
                return DefaultLocalizationHelper.GetLanguage(m_CurrentLanguage);
            }
            set
            {
                SetLanguage(DefaultLocalizationHelper.GetLanguageStr(value));
            }
        }
            
        /// <summary>
        /// 获取系统语言。
        /// </summary>
        public Language SystemLanguage => DefaultLocalizationHelper.SystemLanguage;

        private IResourceManager m_ResourceManager;

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            
            m_ResourceManager = ModuleImpSystem.GetModule<IResourceManager>();
            if (m_ResourceManager == null)
            {
                Log.Fatal("Resource component is invalid.");
                return;
            }
        }

        private void Start()
        {
            RootModule rootModule = ModuleSystem.GetModule<RootModule>();
            if (rootModule == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            m_DefaultLanguage = DefaultLocalizationHelper.GetLanguageStr(
                rootModule.EditorLanguage != Language.Unspecified ? rootModule.EditorLanguage : SystemLanguage);

            AsyncInit().Forget();
        }

        private async UniTask<bool> AsyncInit()
        {
            if (string.IsNullOrEmpty(m_DefaultLanguage))
            {
                Log.Fatal($"Must set defaultLanguage.");
                return false;
            }
#if UNITY_EDITOR
            if (!m_UseRuntimeModule)
            {
                LocalizationManager.RegisterSourceInEditor();
                UpdateAllLanguages();
                SetLanguage(m_DefaultLanguage);
            }
            else
            {
                m_SourceData.Awake();
                await LoadLanguage(m_DefaultLanguage, true, true);
            }
#else
            m_SourceData.Awake();
            await LoadLanguage(m_DefaultLanguage, true, true);
#endif
            return true;
        }

        /// <summary>
        /// 加载语言总表。
        /// </summary>
        public async UniTask LoadLanguageTotalAsset(string assetName)
        {
#if UNITY_EDITOR
            if (!m_UseRuntimeModule)
            {
                Log.Warning($"禁止在此模式下 动态加载语言");
                return;
            }
#endif
            TextAsset assetTextAsset= await m_ResourceManager.LoadAssetAsync<TextAsset>(assetName);
            
            if (assetTextAsset == null)
            {
                Log.Warning($"没有加载到语言总表");
                return;
            }

            Log.Info($"加载语言总表成功");

            UseLocalizationCSV(assetTextAsset.text, true);
        }

        /// <summary>
        /// 加载语言分表。
        /// </summary>
        /// <param name="language">语言类型。</param>
        /// <param name="setCurrent">是否立刻设置成当前语言。</param>
        /// <param name="fromInit">是否初始化Inner语言。</param>
        public async UniTask LoadLanguage(string language, bool setCurrent = false, bool fromInit = false)
        {
#if UNITY_EDITOR
            if (!m_UseRuntimeModule)
            {
                Log.Warning($"禁止在此模式下 动态加载语言 {language}");
                return;
            }
#endif
            TextAsset assetTextAsset;

            if (!fromInit)
            {
                var assetName = GetLanguageAssetName(language);

                assetTextAsset = await m_ResourceManager.LoadAssetAsync<TextAsset>(assetName);
            }
            else
            {
                if (m_InnerLocalizationCSV == null)
                {
                    Log.Warning($"请使用I2Localization.asset导出CSV创建内置多语言.");
                    return;
                }
                
                assetTextAsset = m_InnerLocalizationCSV;
            }

            if (assetTextAsset == null)
            {
                Log.Warning($"没有加载到目标语言资源 {language}");
                return;
            }

            Log.Info($"加载语言成功 {language}");

            UseLocalizationCSV(assetTextAsset.text, !setCurrent);
            if (setCurrent)
            {
                SetLanguage(language);
            }
        }

        private string GetLanguageAssetName(string language)
        {
            return $"{DefaultLocalizationHelper.I2ResAssetNamePrefix}{language}";
        }

        /// <summary>
        /// 检查并初始化所有语言的Id。
        /// </summary>
        private void UpdateAllLanguages()
        {
            m_AllLanguage.Clear();
            List<string> allLanguage = LocalizationManager.GetAllLanguages();
            foreach (var language in allLanguage)
            {
                var newLanguage = Regex.Replace(language, @"[\r\n]", "");
                m_AllLanguage.Add(newLanguage);
            }
        }

        /// <summary>
        /// 检查是否存在该语言。
        /// </summary>
        /// <param name="language">语言。</param>
        /// <returns>是否已加载。</returns>
        public bool CheckLanguage(string language)
        {
            return m_AllLanguage.Contains(language);
        }

        /// <summary>
        /// 设置当前语言。
        /// </summary>
        /// <param name="language">语言名称。</param>
        /// <param name="load">是否加载。</param>
        /// <returns></returns>
        public bool SetLanguage(Language language, bool load = false)
        {
            return SetLanguage(DefaultLocalizationHelper.GetLanguageStr(language), load);
        }

        /// <summary>
        /// 设置当前语言。
        /// </summary>
        /// <param name="language">语言名称。</param>
        /// <param name="load">是否加载。</param>
        /// <returns></returns>
        public bool SetLanguage(string language, bool load = false)
        {
            if (!CheckLanguage(language))
            {
                if (load)
                {
                    LoadLanguage(language, true).Forget();
                    return true;
                }

                Log.Warning($"当前没有这个语言无法切换到此语言 {language}");
                return false;
            }

            if (m_CurrentLanguage == language)
            {
                return true;
            }

            Log.Info($"设置当前语言 = {language}");
            LocalizationManager.CurrentLanguage = language;
            m_CurrentLanguage = language;
            return true;
        }

        /// <summary>
        /// 通过语言的Id设置语言。
        /// </summary>
        /// <param name="languageId">语言ID。</param>
        /// <returns>是否设置成功。</returns>
        public bool SetLanguage(int languageId)
        {
            if (languageId < 0 || languageId >= m_AllLanguage.Count)
            {
                Log.Warning($"Error languageIndex. Could not set and check {languageId}  Language.Count = {m_AllLanguage.Count}.");
                return false;
            }

            var language = m_AllLanguage[languageId];
            return SetLanguage(language);
        }

        private void UseLocalizationCSV(string text, bool isLocalizeAll = false)
        {
            m_SourceData.Import_CSV(string.Empty, text, eSpreadsheetUpdateMode.Merge, ',');
            if (isLocalizeAll)
            {
                LocalizationManager.LocalizeAll();
            }

            UpdateAllLanguages();
        }

        /// <summary>
        /// 语言模块加载资源接口。
        /// </summary>
        /// <param name="path">资源定位地址。</param>
        /// <typeparam name="T">资源类型。</typeparam>
        /// <returns>返回资源实例。</returns>
        public T LoadFromBundle<T>(string path) where T : Object
        {
            var assetObject = m_ResourceManager.LoadAsset<T>(path);
            if (assetObject != null)
            {
                return assetObject;
            }

            Log.Error($"Localization could not load {path}  assetsType :{typeof(T).Name}.");
            return null;
        }
    }
}