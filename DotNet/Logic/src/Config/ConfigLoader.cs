using System.Diagnostics;
using Bright.Serialization;
using GameConfig;
using TEngine;
using TEngine.Core;

/// <summary>
/// 配置加载器。
/// </summary>
public class ConfigLoader:Singleton<ConfigLoader>
{
    private bool _init = false;
    
    private Tables _tables = null!;

    public ConfigLoader()
    {
        this.Load();
    }

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
    /// 加载配置。
    /// </summary>
    public void Load()
    {
        try
        {
            var tablesCtor = typeof(Tables).GetConstructors()[0];
            var loaderReturnType = tablesCtor.GetParameters()[0].ParameterType.GetGenericArguments()[1];

            System.Delegate loader =  new System.Func<string, ByteBuf>(LoadByteBuf);
            _tables = (Tables)tablesCtor.Invoke(new object[] { loader });
        }
        catch (Exception e)
        {
            Log.Warning($"找不到游戏配置 启动项目前请运行Luban目录gen_code_bin_to_server.bat."+e.Message);
        }
    }


    /// <summary>
    /// 加载二进制配置。
    /// </summary>
    /// <param name="file">FileName</param>
    /// <returns>ByteBuf</returns>
    private ByteBuf LoadByteBuf(string file)
    {
         byte[]ret = File.ReadAllBytes($"../../../Config/GameConfig/{file}.bytes");
        return new ByteBuf(ret);
    }
}