using System;
using System.Collections.Generic;
using System.Net;
using kcp2k;
using WeChatWASM;

public class WXSocket : ISocket
{
    private string address;
    private int port;
    private WXUDPSocket socket;

    private Queue<byte[]> recvQueue = new();

    public WXSocket(string address, int port)
    {
        this.address = address;
        this.port = port;

        socket = WX.CreateUDPSocket();
        socket.OnMessage(OnMessageHandler);
        socket.OnError(OnErrorHandler);
        socket.OnClose(OnCloseHandler);
        socket.Bind();
    }

    public bool Blocking
    {
        get => true;
        set
        { /* ignored */
        }
    }

    public EndPoint LocalEndPoint
    {
        get { throw new NotSupportedException(); }
    }

    public int SendBufferSize
    {
        get => 0;
        set
        { /* ignored */
        }
    }

    public int ReceiveBufferSize
    {
        get => 0;
        set
        { /* ignored */
        }
    }

    public void Connect(EndPoint remoteEP)
    {
        throw new NotSupportedException("Use Connect(string address, int port).");
    }

    public bool ReceiveNonBlocking(byte[] recvBuffer, out ArraySegment<byte> data)
    {
        if (recvQueue.Count > 0)
        {
            var packet = recvQueue.Dequeue();
            int len = Math.Min(packet.Length, recvBuffer.Length);
            Array.Copy(packet, 0, recvBuffer, 0, len);
            data = new ArraySegment<byte>(recvBuffer, 0, len);
            return true;
        }

        data = default;
        return false;
    }

    public bool SendNonBlocking(ArraySegment<byte> data)
    {
        socket.Send(
            new UDPSocketSendOption()
            {
                address = this.address,
                port = this.port,
                message = data.Array,
                length = data.Count,
                offset = data.Offset,
            }
        );
        // Log.Info($"[WXSocket] SendNonBlocking, address: {this.address}, port: {port}, message: {BitConverter.ToString(data.Array!, data.Offset, data.Count)}");
        return true;
    }

    private void OnCloseHandler(GeneralCallbackResult result)
    {
        Log.Warning($"[WXSocket] onCloseHandler: {result.errMsg}");
    }

    private void OnErrorHandler(GeneralCallbackResult result)
    {
        Log.Warning($"[WXSocket] onErrorHandler: {result.errMsg}");
    }

    private void OnMessageHandler(UDPSocketOnMessageListenerResult result)
    {
        if (result.message != null)
        {
            recvQueue.Enqueue(result.message);
            // Log.Info( $"[WXSocket] OnMessageHandler, message: {BitConverter.ToString(result.message, result.message.Length)}");
        }
    }

    public void Close()
    {
        socket?.OffMessage(OnMessageHandler);
        socket?.OffError(OnErrorHandler);
        socket?.OffClose(OnCloseHandler);
        socket?.Close();
        socket = null;
    }
}
