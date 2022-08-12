using System;
using System.Threading;
using KcpProject;
using TEngine;
using UnityEngine;

public class KcpTest : MonoBehaviour
{
    private KCPSession connection;
    byte[] buffer = new byte[1500];
    int counter = 0;
    int sendBytes = 0;
    int recvBytes = 0;

    void Start()
    {
        connection = new KCPSession();
        connection.AckNoDelay = true;
        connection.WriteDelay = false;

        connection.Connect("127.0.0.1", 4444);
    }

    // Update is called once per frame
    void Update()
    {
        if (connection == null || !connection.IsConnected)
        {
            return;
        }
        connection.Update();

        //firstSend = false;
        // Console.WriteLine("Write Message...");
        //var text = Encoding.UTF8.GetBytes(string.Format("Hello KCP: {0}", ++counter));
        var sent = connection.Send(buffer, 0, buffer.Length);
        if (sent < 0)
        {
            TLogger.LogError("Write message failed.");
        }

        if (sent > 0)
        {
            counter++;
            sendBytes += buffer.Length;
            if (counter >= 500)
            {

            }
        }

        var n = connection.Recv(buffer, 0, buffer.Length);
        if (n == 0)
        {
            Thread.Sleep(10);
        }
        else if (n < 0)
        {
            TLogger.LogError("Receive Message failed.");
        }
        else
        {
            recvBytes += n;
            TLogger.LogError($"{recvBytes} / {sendBytes}");
        }
    }
}