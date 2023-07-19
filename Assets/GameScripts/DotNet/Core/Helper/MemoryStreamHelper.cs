using System.IO;
using TEngine.IO;

namespace TEngine.Core
{
    public static class MemoryStreamHelper
    {
        private static readonly RecyclableMemoryStreamManager Manager = new RecyclableMemoryStreamManager();
        
        public static MemoryStream GetRecyclableMemoryStream()
        {
            return Manager.GetStream();
        }
    }
}