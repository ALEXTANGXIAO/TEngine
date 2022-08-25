namespace TEngine.Runtime
{
    public static partial class Utility
    {
        public static class Text
        {
            public static string Format(string format, params System.Object[] args)
            {
                return string.Format(format, args);
            }
        }
    }
}
