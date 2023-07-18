#if TENGINE_NET
#pragma warning disable CS8618
namespace TEngine
{
    public static class AppDefine
    {
        public static CommandLineOptions Options;
        public static uint AppId => Options.AppId;
    }
}
#endif