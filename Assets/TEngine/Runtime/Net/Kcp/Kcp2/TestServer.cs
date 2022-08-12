using System;
using System.Threading.Tasks;
using Kcp;
using UnityEngine;

public class TestServer : MonoBehaviour
{
    void Start()
    {
        KcpClient kcpClient = new KcpClient(40001);
        Task.Run(async () =>
        {
            while (true)
            {
                kcpClient.kcp.Update(DateTime.UtcNow);
                await Task.Delay(10);
            }
        });

        StartRecv(kcpClient);
    }

    static async void StartRecv(KcpClient client)
    {
        var res = await client.ReceiveAsync();
        StartRecv(client);

        await Task.Delay(1);
        var str = System.Text.Encoding.UTF8.GetString(res);
        if ("发送一条消息" == str)
        {
            Console.WriteLine(str);

            var buffer = System.Text.Encoding.UTF8.GetBytes("回复一条消息");
            client.SendAsync(buffer, buffer.Length);
        }
    }
}