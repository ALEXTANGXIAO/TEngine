using Bright.Serialization;
using System.IO;
using GameBase;
using GameConfig;
using SimpleJSON;
using UnityEngine;

/// <summary>
/// 配置加载器
/// </summary>
public class ConfigLoader:Singleton<ConfigLoader>
{
    private bool _init = false;
    
    private Tables _tables;
    
    public Tables Tables
    {
        get
        {
            if (!_init)
            {
                _init = true;
                Load();
            }
            return _tables;
        }
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    public void Load()
    {
        var tablesCtor = typeof(Tables).GetConstructors()[0];
        var loaderReturnType = tablesCtor.GetParameters()[0].ParameterType.GetGenericArguments()[1];

        System.Delegate loader = loaderReturnType == typeof(ByteBuf)
            ? new System.Func<string, ByteBuf>(LoadByteBuf)
            : (System.Delegate)new System.Func<string, JSONNode>(LoadJson);
        _tables = (Tables)tablesCtor.Invoke(new object[] { loader });
    }

    /// <summary>
    /// 加载Json配置。
    /// </summary>
    /// <param name="file">FileName</param>
    /// <returns>JSONNode</returns>
    private JSONNode LoadJson(string file)
    {
#if UNITY_EDITOR
        var ret = File.ReadAllText($"{Application.dataPath}/../GenerateDatas/json/{file}.json", System.Text.Encoding.UTF8);
#else
        var textAssets = GameModule.Resource.LoadAsset<TextAsset>(file);
        var ret = textAssets.text;
#endif
        return JSON.Parse(ret);
    }

    /// <summary>
    /// 加载二进制配置。
    /// </summary>
    /// <param name="file">FileName</param>
    /// <returns>ByteBuf</returns>
    private ByteBuf LoadByteBuf(string file)
    {
        byte[] ret = null;
#if UNITY_EDITOR
        ret = File.ReadAllBytes($"{Application.dataPath}/../GenerateDatas/bytes/{file}.bytes");
#else
        var textAssets = GameModule.Resource.LoadAsset<TextAsset>(file);
        ret = textAssets.bytes;
#endif
        return new ByteBuf(ret);
    }
}