using System;
using System.Net;
using System.Net.Sockets;

namespace kcp2k
{
    public class KcpSocket : Socket, ISocket
    {
        public KcpSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) : base(addressFamily, socketType, protocolType)
        {
        }

        public KcpSocket(SocketInformation socketInformation) : base(socketInformation)
        {
        }

        public KcpSocket(SocketType socketType, ProtocolType protocolType) : base(socketType, protocolType)
        {
        }
        
        // non-blocking UDP send.
        // allows for reuse when overwriting KcpServer/Client (i.e. for relays).
        // => wrapped with Poll to avoid WouldBlock allocating new SocketException.
        // => wrapped with try-catch to ignore WouldBlock exception.
        // make sure to set socket.Blocking = false before using this!
        public bool SendNonBlocking(ArraySegment<byte> data)
        {
            try
            {
                // when using non-blocking sockets, SendTo may return WouldBlock.
                // in C#, WouldBlock throws a SocketException, which is expected.
                // unfortunately, creating the SocketException allocates in C#.
                // let's poll first to avoid the WouldBlock allocation.
                // note that this entirely to avoid allocations.
                // non-blocking UDP doesn't need Poll in other languages.
                // and the code still works without the Poll call.
                if (!base.Poll(0, SelectMode.SelectWrite)) return false;

                // SendTo allocates. we used bound Send.
                base.Send(data.Array, data.Offset, data.Count, SocketFlags.None);
                //Log.Info($"[KcpSocket] SendNonBlocking, data: {BitConverter.ToString(data.Array!, data.Offset, data.Count)}");
                return true;
            }
            catch (SocketException e)
            {
                // for non-blocking sockets, SendTo may throw WouldBlock.
                // in that case, simply drop the message. it's UDP, it's fine.
                if (e.SocketErrorCode == SocketError.WouldBlock) return false;

                // otherwise it's a real socket error. throw it.
                throw;
            }
        }

        // non-blocking UDP receive.
        // allows for reuse when overwriting KcpServer/Client (i.e. for relays).
        // => wrapped with Poll to avoid WouldBlock allocating new SocketException.
        // => wrapped with try-catch to ignore WouldBlock exception.
        // make sure to set socket.Blocking = false before using this!
        public bool ReceiveNonBlocking(byte[] recvBuffer, out ArraySegment<byte> data)
        {
            data = default;

            try
            {
                // when using non-blocking sockets, ReceiveFrom may return WouldBlock.
                // in C#, WouldBlock throws a SocketException, which is expected.
                // unfortunately, creating the SocketException allocates in C#.
                // let's poll first to avoid the WouldBlock allocation.
                // note that this entirely to avoid allocations.
                // non-blocking UDP doesn't need Poll in other languages.
                // and the code still works without the Poll call.
                if (!base.Poll(0, SelectMode.SelectRead)) return false;

                // ReceiveFrom allocates. we used bound Receive.
                // returns amount of bytes written into buffer.
                // throws SocketException if datagram was larger than buffer.
                // https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.receive?view=net-6.0
                //
                // throws SocketException if datagram was larger than buffer.
                // https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.socket.receive?view=net-6.0
                int size = base.Receive(recvBuffer, 0, recvBuffer.Length, SocketFlags.None);
                //if (size > 0) Log.Info($"[KcpSocket] ReceiveNonBlocking, data: {BitConverter.ToString(recvBuffer, 0, size)}");
                data = new ArraySegment<byte>(recvBuffer, 0, size);
                return true;
            }
            catch (SocketException e)
            {
                // for non-blocking sockets, Receive throws WouldBlock if there is
                // no message to read. that's okay. only log for other errors.
                if (e.SocketErrorCode == SocketError.WouldBlock) return false;

                // otherwise it's a real socket error. throw it.
                throw;
            }
        }
    }
}
