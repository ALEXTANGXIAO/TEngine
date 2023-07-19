using System;
using System.Threading.Tasks;

namespace TEngine.Core
{
    public interface ISingleton : IDisposable
    {
        public bool IsDisposed { get; set; }
        public Task Initialize();
    }
}