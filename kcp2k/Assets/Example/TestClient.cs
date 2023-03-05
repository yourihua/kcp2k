﻿using System;
using UnityEngine;

namespace kcp2k.Examples
{
    public class TestClient : MonoBehaviour
    {
        // configuration
        public ushort Port = 7777;
        public KcpConfig config = new KcpConfig();

        // client
        public KcpClient client;

        // MonoBehaviour ///////////////////////////////////////////////////////
        void Awake()
        {
            // logging
            Log.Info = Debug.Log;
            Log.Warning = Debug.LogWarning;
            Log.Error = Debug.LogError;

            client = new KcpClient(
                () => {},
                (message, channel) => Debug.Log($"KCP: OnClientDataReceived({BitConverter.ToString(message.Array, message.Offset, message.Count)} @ {channel})"),
                () => {},
                (error, reason) => Debug.LogWarning($"KCP: OnClientError({error}, {reason}"),
                config
            );
        }

        public void LateUpdate() => client.Tick();

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(5, 5, 150, 400));
            GUILayout.Label("Client:");
            if (GUILayout.Button("Connect 127.0.0.1"))
            {
                client.Connect("127.0.0.1", Port);
            }
            if (GUILayout.Button("Send 0x01, 0x02 reliable"))
            {
                client.Send(new ArraySegment<byte>(new byte[]{0x01, 0x02}), KcpChannel.Reliable);
            }
            if (GUILayout.Button("Send 0x03, 0x04 unreliable"))
            {
                client.Send(new ArraySegment<byte>(new byte[]{0x03, 0x04}), KcpChannel.Unreliable);
            }
            if (GUILayout.Button("Disconnect"))
            {
                client.Disconnect();
            }
            GUILayout.EndArea();
        }
    }
}