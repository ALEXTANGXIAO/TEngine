using System.Diagnostics;
using Bright.Serialization;
using GameConfig;
using TEngine;
using TEngine.Core;
using TEngine.Helper;

/// <summary>
/// 配置加载器。
/// </summary>
public class ConfigLoader:Singleton<ConfigLoader>
{
    private bool _init = false;
    
    private Tables _tables = null!;

    public Tables Tables
    {
        get
        {
            if (!_init)
            {
                Log.Error("Config not loaded.");
            }
            return _tables;
        }
    }

    /// <summary>
    /// 加载配置。
    /// </summary>
    public async Task LoadAsync()
    {
        try
        {
            _tables = new Tables();
            await _tables.LoadAsync(LoadByteBuf);
            _init = true;
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
    private async Task<ByteBuf> LoadByteBuf(string file)
    {
#if false
        GameTickWatcher gameTickWatcher = new GameTickWatcher();
#endif
        var ret = await File.ReadAllBytesAsync($"../../../Config/GameConfig/{file}.bytes");
#if false
        Log.Warning($"LoadByteBuf {file} used time {gameTickWatcher.ElapseTime()}");
#endif
        return new ByteBuf(ret);
    }
}