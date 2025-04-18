using System;
using System.Net;
using System.Net.Sockets;

namespace kcp2k
{
    public class KcpSocket : ISocket
    {
        private Socket socket;

        public EndPoint LocalEndPoint
        {
            get { return socket.LocalEndPoint; }
        }

        public bool Blocking
        {
            get { return socket.Blocking; }
            set { socket.Blocking = value; }
        }

        public int ReceiveBufferSize
        {
            get { return socket.ReceiveBufferSize; }
            set { socket.ReceiveBufferSize = value; }
        }

        public int SendBufferSize
        {
            get { return socket.SendBufferSize; }
            set { socket.SendBufferSize = value; }
        }

        public KcpSocket(
            AddressFamily addressFamily,
            SocketType socketType,
            ProtocolType protocolType
        )
        {
            socket = new Socket(addressFamily, socketType, protocolType);
        }

        public void Connect(EndPoint remoteEP)
        {
            socket.Connect(remoteEP);
        }

        public void Close()
        {
            socket.Close();
        }

        public bool SendNonBlocking(ArraySegment<byte> data)
        {
            return socket.SendNonBlocking(data);
        }

        public bool ReceiveNonBlocking(byte[] recvBuffer, out ArraySegment<byte> data)
        {
            return socket.ReceiveNonBlocking(recvBuffer, out data);
        }
    }
}
