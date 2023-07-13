public static class AssemblyName
{
    // 这个对应的是NetCore工程
    public const int AssemblyDotNet = 1;
    
    public const int GameBase = 10;
    
    public const int GameLogic = 20;
    
    public const int GameProto = 30;

    // 你可以添加多个工程、如果有新添加的可以在这里添加、
    // 并在AssemblyLoadHelper里添加对应的加载逻辑
    // 参考LoadModelDll这个方法
    // 这样添加是为了方便后面热重载使用
}
