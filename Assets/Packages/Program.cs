using System;
using System.Buffers;
using System.Net.Sockets.Kcp;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestKCP
{
    public class Handle : IKcpCallback
    {
        //public void Output(ReadOnlySpan<byte> buffer)
        //{
        //    var frag = new byte[buffer.Length];
        //    buffer.CopyTo(frag);
        //    Out(frag);
        //}

        public Action<Memory<byte>> Out;
        public Action<byte[]> Recv;
        public void Receive(byte[] buffer)
        {
            Recv(buffer);
        }

        public IMemoryOwner<byte> RentBuffer(int lenght)
        {
            return null;
        }

        public void Output(IMemoryOwner<byte> buffer, int avalidLength)
        {
            using (buffer)
            {
                Out(buffer.Memory.Slice(0, avalidLength));
            }
        }
    }

    public class TestClass
    {
        public const string message =
        #region MyRegion

            @"LICENSE SYSTEM [2017918 10:58:53] Next license update check is after 2025-06-30T00:00:00

Built from '5.5/release' branch; Version is '5.5.0f3 (38b4efef76f0) revision 3716335'; Using compiler version '160040219'
OS: 'Windows 7 Service Pack 1 (6.1.7601) 64bit' Language: 'zh' Physical Memory: 16224 MB
BatchMode: 0, IsHumanControllingUs: 1, StartBugReporterOnCrash: 1, Is64bit: 1, IsPro: 1
Initialize mono
Mono path[0] = 'C:/Program Files/Unity5.5.0/Editor/Data/Managed'
Mono path[1] = 'C:/Program Files/Unity5.5.0/Editor/Data/Mono/lib/mono/2.0'
Mono path[2] = 'C:/Program Files/Unity5.5.0/Editor/Data/UnityScript'
Mono config path = 'C:/Program Files/Unity5.5.0/Editor/Data/Mono/etc'
Using monoOptions --debugger-agent=transport=dt_socket,embedding=1,defer=y,address=0.0.0.0:56392
IsTimeToCheckForNewEditor: Update time 1505705705 current 1505703540
C:/work/irobotqv2.0_dev/irobotqv2.0_app
Loading GUID <-> Path mappings...0.000281 seconds
Loading Asset Database...0.015599 seconds
Audio: FMOD Profiler initialized on port 54900
AssetDatabase consistency checks...0.019115 seconds
Initialize engine version: 5.5.0f3 (38b4efef76f0)
GfxDevice: creating device client; threaded=1
Direct3D:
    Version:  Direct3D 11.0 [level 11.0]
    Renderer: AMD Radeon HD 6670 (ID=0x6758)
    Vendor:   ATI
    VRAM:     4418 MB
    Driver:   14.100.0.0
Begin MonoManager ReloadAssembly1
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\UnityEngine.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\UnityEditor.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\Unity.Locator.dll (this message is harmless)
Refreshing native plugins compatible for Editor in 9.17 ms, found 3 plugins.
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\I18N.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\I18N.CJK.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\Unity.DataContract.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.Core.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\Unity.IvyParser.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.Xml.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.Configuration.dll (this message is harmless)
Begin MonoManager ReloadAssembly2
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\UnityEngine.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\UnityEditor.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\Unity.Locator.dll (this message is harmless)
Refreshing native plugins compatible for Editor in 9.17 ms, found 3 plugins.
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\I18N.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\I18N.CJK.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\Unity.DataContract.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.Core.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\Unity.IvyParser.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.Xml.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.Configuration.dll (this message is harmless)
Begin MonoManager ReloadAssembly3
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\UnityEngine.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\UnityEditor.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\Unity.Locator.dll (this message is harmless)
Refreshing native plugins compatible for Editor in 9.17 ms, found 3 plugins.
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\I18N.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\I18N.CJK.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\Unity.DataContract.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.Core.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\Unity.IvyParser.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.Xml.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.Configuration.dll (this message is harmless)
Begin MonoManager ReloadAssembly4
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\UnityEngine.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\UnityEditor.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\Unity.Locator.dll (this message is harmless)
Refreshing native plugins compatible for Editor in 9.17 ms, found 3 plugins.
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\I18N.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\I18N.CJK.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\Unity.DataContract.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.Core.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\Unity.IvyParser.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.Xml.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.Configuration.dll (this message is harmless)
Begin MonoManager ReloadAssembly5
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\UnityEngine.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\UnityEditor.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\Unity.Locator.dll (this message is harmless)
Refreshing native plugins compatible for Editor in 9.17 ms, found 3 plugins.
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\I18N.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\I18N.CJK.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\Unity.DataContract.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.Core.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Managed\Unity.IvyParser.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.Xml.dll (this message is harmless)
Platform assembly: C:\Program Files\Unity5.5.0\Editor\Data\Mono\lib\mono\2.0\System.Configuration.dll (this message is harmless)";

        #endregion


    }

    public class Program
    {
        static string ShowThread
        {
            get
            {
                return $"  ThreadID[{Thread.CurrentThread.ManagedThreadId}]";
            }
        }


        public static void Main(string[] args)
        {
            Console.WriteLine(ShowThread);
            Random random = new Random();

            var handle1 = new Handle();
            var handle2 = new Handle();

            const int conv = 123;
            var kcp1 = new Kcp(conv, handle1);
            var kcp2 = new Kcp(conv, handle2);

            kcp1.NoDelay(1, 10, 2, 1);//fast
            kcp1.WndSize(64, 64);
            kcp1.SetMtu(512);

            kcp2.NoDelay(1, 10, 2, 1);//fast
            kcp2.WndSize(64, 64);
            kcp2.SetMtu(512);

            var sendbyte = Encoding.ASCII.GetBytes(TestClass.message);

            handle1.Out += buffer =>
            {
                var next = random.Next(100);
                if (next >= 15)///随机丢包
                {
                    //Console.WriteLine($"11------Thread[{Thread.CurrentThread.ManagedThreadId}]");
                    Task.Run(() =>
                    {
                        //Console.WriteLine($"12------Thread[{Thread.CurrentThread.ManagedThreadId}]");
                        kcp2.Input(buffer.Span);
                    });

                }
                else
                {
                    //Console.WriteLine("Send miss");
                }
            };

            handle2.Out += buffer =>
            {
                var next = random.Next(100);
                if (next >= 0)///随机丢包
                {
                    Task.Run(() =>
                    {
                        kcp1.Input(buffer.Span);
                    });
                }
                else
                {
                    Console.WriteLine("Resp miss");
                }
            };
            int count = 0;

            handle1.Recv += buffer =>
            {
                var str = Encoding.ASCII.GetString(buffer);
                count++;
                if (TestClass.message == str)
                {
                    kcptest.Log1($"kcp  echo----{count}");
                }
                var res = kcp1.Send(buffer);
                if (res != 0)
                {
                    kcptest.Log1($"kcp send error");
                }
            };

            int recvCount = 0;

            handle2.Recv += buffer =>
            {
                recvCount++;
                kcptest.Log2($"kcp2 recv----{recvCount}");
                var res = kcp2.Send(buffer);
                if (res != 0)
                {
                    kcptest.Log2($"kcp send error");
                }
            };

            Task.Run(async () =>
            {
                try
                {
                    int updateCount = 0;
                    while (true)
                    {
                        kcp1.Update(DateTime.UtcNow);

                        int len;
                        while ((len = kcp1.PeekSize()) > 0)
                        {
                            var buffer = new byte[len];
                            if (kcp1.Recv(buffer) >= 0)
                            {
                                handle1.Receive(buffer);
                            }
                        }

                        await Task.Delay(5);
                        updateCount++;
                        if (updateCount % 1000 == 0)
                        {
                            Console.WriteLine($"KCP1 ALIVE {updateCount}----{ShowThread}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            });

            Task.Run(async () =>
            {
                try
                {
                    int updateCount = 0;
                    while (true)
                    {
                        kcp2.Update(DateTime.UtcNow);

                        //var utcNow = DateTime.UtcNow;
                        //var res = kcp2.Check(utcNow);

                        int len;
                        do
                        {
                            var (buffer, avalidSzie) = kcp2.TryRecv();
                            len = avalidSzie;
                            if (buffer != null)
                            {
                                var temp = new byte[len];
                                buffer.Memory.Span.Slice(0, len).CopyTo(temp);
                                handle2.Receive(temp);
                            }
                        } while (len > 0);

                        await Task.Delay(5);
                        updateCount++;
                        if (updateCount % 1000 == 0)
                        {
                            Console.WriteLine($"KCP2 ALIVE {updateCount}----{ShowThread}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            kcp1.Send(sendbyte);

            
        }
    }
}
