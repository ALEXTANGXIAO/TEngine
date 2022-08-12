using System;
using System.Net;
using System.Threading.Tasks;
using Kcp;
using TEngine;
using UnityEngine;

public class TestClient : MonoBehaviour
{
    static IPEndPoint end = new System.Net.IPEndPoint(System.Net.IPAddress.Loopback, 40001);
    private KcpClient kcpClient;
    private Task task;
    void Start()
    {
        kcpClient = new KcpClient(50001, end);
        task = Task.Run(async () =>
        {
            while (true)
            {
                kcpClient.kcp.Update(DateTime.UtcNow);
                await Task.Delay(10);
            }
        });
    }

    static async void Send(KcpClient client, string v)
    {
        var buffer = System.Text.Encoding.UTF8.GetBytes(v);
        client.SendAsync(buffer, buffer.Length);
        var resp = await client.ReceiveAsync();
        var respstr = System.Text.Encoding.UTF8.GetString(resp);
        TLogger.LogError($"收到服务器回复:    {respstr}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TLogger.LogError($"发送一条消息");
            Send(kcpClient, "发送一条消息");
        }
    }
}
