using System.Collections.Generic;
using System.Threading;
using Bright.Serialization;
using Cysharp.Threading.Tasks;
using GameBase;
using GameConfig;
using TEngine;
using UnityEngine;

/// <summary>
/// 配置加载器
/// </summary>
public class ConfigSystem : Singleton<ConfigSystem>
{
    private bool _init = false;

    private Tables _tables;

    public Tables Tables
    {
        get
        {
            if (!_init)
            {
#if !UNITY_WEBGL
                _init = true;

#endif
                Log.Error("Config not loaded. You need Take LoadAsync at first.");
            }
            return _tables;
        }
    }

    private readonly Dictionary<string, TextAsset> _configs = new Dictionary<string, TextAsset>();

    /// <summary>
    /// 异步加载配置。
    /// </summary>
    public async UniTask LoadAsync()
    {
        _tables = new Tables();
        await _tables.LoadAsync(LoadByteBufAsync);
        _init = true;
    }

    /// <summary>
    /// 异步加载二进制配置。
    /// </summary>
    /// <param name="file">FileName</param>
    /// <returns>ByteBuf</returns>
    private async UniTask<ByteBuf> LoadByteBufAsync(string file)
    {
#if false
        GameTickWatcher gameTickWatcher = new GameTickWatcher();
#endif
        byte[] ret;
        var location = file;
        if (_configs.TryGetValue(location, out var config))
        {
            ret = config.bytes;
        }
        else
        {
            var textAssets = await GameModule.Resource.LoadAssetAsync<TextAsset>(location, CancellationToken.None);
            ret = textAssets.bytes;
            RegisterTextAssets(file, textAssets);
        }
#if false
        Log.Warning($"LoadByteBuf {file} used time {gameTickWatcher.ElapseTime()}");
#endif
        return new ByteBuf(ret);
    }

    /// <summary>
    /// 注册配置资源。
    /// </summary>
    /// <param name="key">资源Key。</param>
    /// <param name="value">资源实例。</param>
    /// <returns>注册成功。</returns>
    private bool RegisterTextAssets(string key, TextAsset value)
    {
        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        if (value == null)
        {
            return false;
        }
        _configs[key] = value;
        return true;
    }
}