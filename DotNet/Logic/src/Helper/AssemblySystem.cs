#if TENGINE_NET
using TEngine.Core;
#pragma warning disable CS8603

namespace TEngine;

/// <summary>
/// 整个框架使用的程序集、有几个程序集就定义集。这里定义是为了后面方面使用
/// </summary>
public static class AssemblyName
{
    public const int Hotfix = 1;
}

public static class AssemblySystem
{
    public static void Init()
    {
        LoadHotfix();
    }

    public static void LoadHotfix()
    {
        AssemblyManager.Load(AssemblyName.Hotfix, typeof(AssemblySystem).Assembly);
    }
}
#endif