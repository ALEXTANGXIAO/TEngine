using Luban;
using GameBase;
using GameConfig;
using TEngine;
using UnityEngine;

/// <summary>
/// 配置加载器。
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
                Load();
            }

            return _tables;
        }
    }

    /// <summary>
    /// 加载配置。
    /// </summary>
    public void Load()
    {
        _tables = new Tables(LoadByteBuf);
        _init = true;
    }

    /// <summary>
    /// 加载二进制配置。
    /// </summary>
    /// <param name="file">FileName</param>
    /// <returns>ByteBuf</returns>
    private ByteBuf LoadByteBuf(string file)
    {
        TextAsset textAsset = null;
        textAsset = GameModule.Resource.GetPreLoadAsset<TextAsset>(file);
        if (textAsset != null)
        {
            return new ByteBuf(textAsset.bytes);
        }
        else
        {
            textAsset = GameModule.Resource.LoadAsset<TextAsset>(file);
            return new ByteBuf(textAsset.bytes);
        }
    }
}