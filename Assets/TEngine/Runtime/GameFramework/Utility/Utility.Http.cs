using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace TEngine
{
    public static partial class Utility
    {
        /// <summary>
        /// Http 相关的实用函数。
        /// </summary>
        public static partial class Http
        {
            /// <summary> 
            /// GET请求与获取结果.
            /// </summary> 
            public static async UniTask<string> Get(string url,float timeout = 5f)
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfterSlim(TimeSpan.FromSeconds(timeout));

                UnityWebRequest unityWebRequest = UnityWebRequest.Get(url);
                try
                {
                    var (isCanceled, _) = await unityWebRequest.SendWebRequest().WithCancellation(cts.Token).SuppressCancellationThrow();
                    if (isCanceled)
                    {
                        Log.Warning($"HttpGet {url} be canceled!");
                    }
                }
                catch (OperationCanceledException ex)
                {
                    if (ex.CancellationToken == cts.Token)
                    {
                        Log.Debug("HttpGet Timeout");
                        return string.Empty;
                    }
                }
                return unityWebRequest.downloadHandler.text;
            }
        }
    }
}