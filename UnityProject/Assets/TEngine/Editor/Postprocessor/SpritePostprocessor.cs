using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 图集导入管线。
/// </summary>
public class SpritePostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (var s in importedAssets)
        {
            EditorSpriteSaveInfo.OnImportSprite(s);
        }

        foreach (var s in deletedAssets)
        {
            EditorSpriteSaveInfo.OnDeleteSprite(s);
        }

        foreach (var s in movedFromAssetPaths)
        {
            EditorSpriteSaveInfo.OnDeleteSprite(s);
        }

        foreach (var s in movedAssets)
        {
            EditorSpriteSaveInfo.OnImportSprite(s);
        }
    }
}

public static class EditorSpriteSaveInfo
{
    private const string NormalAtlasDir = "Assets/AssetArt/Atlas";
    private const string UISpritePath = "Assets/AssetRaw/UIRaw";
    private const string UIAtlasPath = "Assets/AssetRaw/UIRaw/Atlas";
    private const string UIRawPath = "Assets/AssetRaw/UIRaw/UIRaw";
    private static List<string> m_dirtyAtlasList = new List<string>();
    private static Dictionary<string, List<string>> m_allASprites = new Dictionary<string, List<string>>();
    private static Dictionary<string, string> m_uiAtlasMap = new Dictionary<string, string>();
    private static bool m_inited = false;
    private static bool m_dirty = false;

    public static void Init()
    {
        if (m_inited)
        {
            return;
        }

        EditorApplication.update += CheckDirty;
    }

    public static void CheckDirty()
    {
        if (m_dirty)
        {
            m_dirty = false;

            AssetDatabase.Refresh();
            float lastProgress = -1;
            for (int i = 0; i < m_dirtyAtlasList.Count; i++)
            {
                string atlasName = m_dirtyAtlasList[i];
                Debug.Log("更新图集 : " + atlasName);
                var curProgress = (float)i / m_dirtyAtlasList.Count;
                if (curProgress > lastProgress + 0.01f)
                {
                    lastProgress = curProgress;
                    var progressText = $"当前进度：{i}/{m_dirtyAtlasList.Count} {atlasName}";
                    bool cancel = EditorUtility.DisplayCancelableProgressBar("刷新图集" + atlasName, progressText, curProgress);
                    if (cancel)
                    {
                        break;
                    }
                }

                bool isUI = atlasName.StartsWith("UIRaw");
                SaveAtlas(atlasName, isUI);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            m_dirtyAtlasList.Clear();
        }
    }

    public static void OnImportSprite(string assetPath)
    {
        if (!assetPath.StartsWith(UISpritePath))
        {
            return;
        }

        TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;

        if (ti != null)
        {
            var modify = false;

            if (assetPath.StartsWith(UISpritePath))
            {
                if (ti.textureType != TextureImporterType.Sprite)
                {
                    ti.textureType = TextureImporterType.Sprite;
                    modify = true;
                }

                if (!string.IsNullOrEmpty(ti.spritePackingTag))
                {
                    ti.spritePackingTag = string.Empty;
                    modify = true;
                }

                var setting = new TextureImporterSettings();
                ti.ReadTextureSettings(setting);
                if (setting.spriteGenerateFallbackPhysicsShape)
                {
                    setting.spriteGenerateFallbackPhysicsShape = false;
                    ti.SetTextureSettings(setting);
                    modify = true;
                }

                if (IsKeepRawImage(assetPath))
                {
                    //调整android格式
                    var andPlatformSettings = ti.GetPlatformTextureSettings("Android");
                    if (!andPlatformSettings.overridden)
                    {
                        andPlatformSettings.overridden = true;
                        modify = true;
                    }

                    if (andPlatformSettings.format != TextureImporterFormat.ASTC_6x6)
                    {
                        andPlatformSettings.format = TextureImporterFormat.ASTC_6x6;
                        ti.SetPlatformTextureSettings(andPlatformSettings);
                        modify = true;
                    }

                    //调整ios格式
                    var iosPlatformSettings = ti.GetPlatformTextureSettings("iPhone");
                    if (!iosPlatformSettings.overridden)
                    {
                        iosPlatformSettings.overridden = true;
                        modify = true;
                    }

                    if (iosPlatformSettings.format != TextureImporterFormat.ASTC_5x5)
                    {
                        iosPlatformSettings.format = TextureImporterFormat.ASTC_5x5;
                        iosPlatformSettings.compressionQuality = 50;
                        ti.SetPlatformTextureSettings(iosPlatformSettings);
                        modify = true;
                    }
                }
            }

            if (modify)
            {
                ti.SaveAndReimport();
            }

            if (ti.textureType == TextureImporterType.Sprite)
            {
                OnProcessSprite(assetPath);
            }
        }
    }

    /// <summary>
    /// 是否保持散图（不打图集）
    /// </summary>
    /// <param name="dirPath"></param>
    /// <returns></returns>
    public static bool IsKeepRawImage(string dirPath)
    {
        return dirPath.Contains("UIRaw/Raw/") || dirPath.Contains("UIRaw_Raw_");
    }

    public static string GetSpritePath(string assetPath)
    {
        string path = assetPath.Substring(0, assetPath.LastIndexOf("."));
        path = path.Replace("Assets/AssetRaw/", "");
        return path;
    }

    /// <summary>
    /// 根据文件路径，返回图集名称
    /// </summary>
    /// <param name="fullName"></param>
    /// <returns></returns>
    public static string GetPackageTag(string fullName)
    {
        fullName = fullName.Replace("\\", "/");
        int idx = fullName.LastIndexOf("UIRaw", StringComparison.Ordinal);
        if (idx == -1)
        {
            return "";
        }

        if (IsKeepRawImage(fullName))
        {
            return "";
        }

        var atlasPath = fullName.Substring(idx);
        string str = atlasPath;
        str = str.Substring(0, str.LastIndexOf("/", StringComparison.Ordinal)).Replace("/", "_");

        return str;
    }

    public static void OnProcessSprite(string assetPath)
    {
        if (!assetPath.StartsWith("Assets"))
        {
            return;
        }

        if (assetPath.StartsWith("Assets/UIRaw_Delete"))
        {
            return;
        }

        Init();

        var spriteName = Path.GetFileNameWithoutExtension(assetPath);
        var spritePath = GetSpritePath(assetPath);
        if (!m_uiAtlasMap.TryGetValue(spriteName, out string oldAssetPath) || spritePath == oldAssetPath)
        {
            m_uiAtlasMap[spriteName] = spritePath;
            m_dirty = true;
        }
        else
        {
            Debug.LogError($"有重名的图片：{spriteName}\n旧图集：{oldAssetPath}\n新图集：{spritePath} ");
            m_uiAtlasMap[spriteName] = spritePath;
            m_dirty = true;
        }

        string atlasName = GetPackageTag(assetPath);
        if (string.IsNullOrEmpty(atlasName))
        {
            bool keepRaw = IsKeepRawImage(assetPath);
            if (!keepRaw)
            {
                Debug.LogError($"empty packingTag of asset :{assetPath} !!!");
            }

            return;
        }
        else
        {
            List<string> ret;
            if (!m_allASprites.TryGetValue(atlasName, out ret))
            {
                ret = new List<string>();
                m_allASprites.Add(atlasName, ret);
            }

            if (!ret.Contains(assetPath))
            {
                ret.Add(assetPath);
                m_dirty = true;
                if (!m_dirtyAtlasList.Contains(atlasName))
                {
                    m_dirtyAtlasList.Add(atlasName);
                }
            }
        }
    }

    public static void OnDeleteSprite(string assetPath)
    {
        if (assetPath.StartsWith("Assets/UIRaw_Delete"))
        {
            return;
        }

        if (!assetPath.StartsWith(UISpritePath))
        {
            return;
        }

        Init();
        string atlasName = GetPackageTag(assetPath);
        if (!m_allASprites.TryGetValue(atlasName, out var ret))
        {
            return;
        }

        //改成文件名的匹配
        if (!ret.Exists(s => Path.GetFileName(s) == Path.GetFileName(assetPath)))
        {
            return;
        }

        if (assetPath.StartsWith(UISpritePath))
        {
            var spriteName = Path.GetFileNameWithoutExtension(assetPath);
            if (m_uiAtlasMap.ContainsKey(spriteName))
            {
                m_uiAtlasMap.Remove(spriteName);
                m_dirty = true;
            }
        }

        ret.Remove(assetPath);
        m_dirty = true;
        if (!m_dirtyAtlasList.Contains(atlasName))
        {
            m_dirtyAtlasList.Add(atlasName);
        }
    }

    #region 更新图集

    public static void SaveAtlas(string atlasName, bool isUI)
    {
        List<Object> spriteList = new List<Object>();
        if (m_allASprites.TryGetValue(atlasName, out var list))
        {
            list.Sort(StringComparer.Ordinal);

            foreach (var s in list)
            {
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(s);
                if (sprite != null)
                {
                    spriteList.Add(sprite);
                }
            }
        }

        var pathv2 = $"{NormalAtlasDir}/{atlasName}.spriteatlasv2";
        var path = $"{NormalAtlasDir}/{atlasName}.asset";
        
        if (spriteList.Count == 0)
        {
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }
            if (File.Exists(pathv2))
            {
                AssetDatabase.DeleteAsset(pathv2);
            }

            return;
        }

        var atlas = new SpriteAtlasAsset();
        var setting = new SpriteAtlasPackingSettings
        {
            blockOffset = 1,
            padding = 2,
            enableRotation = true
        };

        bool isOpaque = atlasName.Contains("Opaque");

        var textureSetting = new SpriteAtlasTextureSettings
        {
            generateMipMaps = false,
            sRGB = true,
            filterMode = FilterMode.Bilinear
        };
        atlas.SetTextureSettings(textureSetting);

        var iphonePlatformSetting = atlas.GetPlatformSettings("iPhone");
        if (!iphonePlatformSetting.overridden)
        {
            iphonePlatformSetting.overridden = true;
            iphonePlatformSetting.format = isOpaque ? TextureImporterFormat.ASTC_5x5 : TextureImporterFormat.ASTC_5x5;
            iphonePlatformSetting.compressionQuality = 100;
            atlas.SetPlatformSettings(iphonePlatformSetting);
        }

        var androidPlatformSetting = atlas.GetPlatformSettings("Android");
        if (isOpaque && !androidPlatformSetting.overridden)
        {
            androidPlatformSetting.overridden = true;
            androidPlatformSetting.format = TextureImporterFormat.ETC_RGB4;
            androidPlatformSetting.compressionQuality = 100;
            atlas.SetPlatformSettings(androidPlatformSetting);
        }

        atlas.SetPackingSettings(setting);
        atlas.Add(spriteList.ToArray());

        AssetDatabase.CreateAsset(atlas, path);
        File.Move(path, pathv2);
        AssetDatabase.Refresh();
    }

    #endregion

    #region 重新生成图集

    private static Dictionary<string, List<string>> m_tempAllASprites = new Dictionary<string, List<string>>();

    [MenuItem("TEngine/图集/重新生成UI图集")]
    static void ForceGenAtlas()
    {
        Init();
        List<string> needSaveAtlas = new List<string>();
        m_tempAllASprites.Clear();
        var findAssets = AssetDatabase.FindAssets("t:sprite", new[] { UIAtlasPath });
        foreach (var findAsset in findAssets)
        {
            var path = AssetDatabase.GUIDToAssetPath(findAsset);
            var atlasName = GetPackageTag(path);
            if (!m_tempAllASprites.TryGetValue(atlasName, out var spriteList))
            {
                spriteList = new List<string>();
                m_tempAllASprites[atlasName] = spriteList;
            }

            if (!spriteList.Contains(path))
            {
                spriteList.Add(path);
            }
        }

        //有变化的才刷
        var iter = m_tempAllASprites.GetEnumerator();
        while (iter.MoveNext())
        {
            bool needSave = false;
            var atlasName = iter.Current.Key;
            var newSpritesList = iter.Current.Value;

            if (m_allASprites.TryGetValue(atlasName, out var existSprites))
            {
                if (existSprites.Count != newSpritesList.Count)
                {
                    needSave = true;
                    existSprites.Clear();
                    existSprites.AddRange(newSpritesList);
                }
                else
                {
                    for (int i = 0; i < newSpritesList.Count; i++)
                    {
                        if (!existSprites.Contains(newSpritesList[i]))
                        {
                            needSave = true;
                            break;
                        }
                    }

                    if (needSave)
                    {
                        existSprites.Clear();
                        existSprites.AddRange(newSpritesList);
                    }
                }
            }
            else
            {
                needSave = true;
                m_allASprites.Add(atlasName, new List<string>(newSpritesList));
            }

            if (needSave && !needSaveAtlas.Contains(atlasName))
            {
                needSaveAtlas.Add(atlasName);
            }
        }

        iter.Dispose();
        foreach (var atlas in needSaveAtlas)
        {
            Debug.LogFormat("Gen atlas:{0}", atlas);
            SaveAtlas(atlas, true);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);
        Debug.Log("Gen end");
    }

    #endregion
}