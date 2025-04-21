using System;
using System.Net;

namespace kcp2k
{
    public interface ISocket
    {
        EndPoint LocalEndPoint { get; }
        bool Blocking { get; set; }
        int ReceiveBufferSize { get; set; }
        int SendBufferSize { get; set; }
        void Connect(EndPoint remoteEP);
        void Close();
        bool SendNonBlocking(ArraySegment<byte> data);
        bool ReceiveNonBlocking(byte[] recvBuffer, out ArraySegment<byte> data);
    }
}
