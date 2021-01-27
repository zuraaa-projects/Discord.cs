﻿using FarDragi.DiscordCs.Gateway.Payloads;
using FarDragi.DiscordCs.Json.Entities.HelloModels;
using FarDragi.DiscordCs.Json.Entities.IdentifyModels;
using FarDragi.DiscordCs.Json.Entities.ResumeModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;

namespace FarDragi.DiscordCs.Gateway.Socket
{
    public class WebSocketClient : IDisposable
    {
        private WebSocket socket;
        private readonly WebSocketDecompress decompress;
        private readonly GatewayClient gatewayClient;
        private readonly JsonIdentify identify;
        private readonly WebSocketConfig config;

        private CancellationTokenSource tokenSource;
        private int sequenceNumber;
        private bool firstConnection;

        public WebSocketClient(GatewayClient gatewayClient, JsonIdentify identify)
        {
            this.gatewayClient = gatewayClient;
            this.identify = identify;
            decompress = new WebSocketDecompress();
            firstConnection = true;
            config = new WebSocketConfig
            {
                Version = 8,
                Encoding = "json"
            };
            AddEvents();
        }

        private void AddEvents()
        {
            socket = new WebSocket(config.Url);
            socket.OnOpen += Socket_OnOpen;
            socket.OnMessage += Socket_OnMessage;
            socket.OnError += Socket_OnError;
            socket.OnClose += Socket_OnClose;
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Socket_OnError(object sender, ErrorEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Socket_OnOpen(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Socket_Closed(object sender, EventArgs e)
        {
            if (e is ClosedEventArgs args)
            {
                Console.WriteLine($"Code: {args.Code} Reason: {args.Reason}\n");

                socket.Dispose();
                AddEvents();
                socket.Open();

                Send(new ResumePayload()
                {
                    Data = new JsonResume
                    {
                        SequenceNumber = sequenceNumber,
                        SessionId = gatewayClient.SessionId,
                        Token = identify.Token
                    }
                });
            }
        }

        private void Socket_Error(object sender, ErrorEventArgs e)
        {
        }

        private void Socket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private void Socket_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (decompress.TryDecompress(e.Data, out string json))
            {
                Payload<object> payload = JsonConvert.DeserializeObject<Payload<object>>(json);
                if (payload.SequenceNumber != null)
                {
                    sequenceNumber = (int)payload.SequenceNumber;
                }

                Console.WriteLine(json);
                Console.WriteLine();

                switch (payload.OpCode)
                {
                    case PayloadOpCode.Dispatch:
                        gatewayClient.OnEventReceived(payload.Event, (JObject)payload.Data, json);
                        break;
                    case PayloadOpCode.Hello:
                        tokenSource = new CancellationTokenSource();
                        Heartbeat((payload.Data as JObject).ToObject<JsonHello>(), tokenSource.Token);
                        break;
                    case PayloadOpCode.Reconnect:
                        break;
                    case PayloadOpCode.InvalidSession:
                        break;
                    case PayloadOpCode.HeartbeatACK:
                        break;
                    default:
                        break;
                }
            }
        }

        private void Socket_Opened(object sender, EventArgs e)
        {
            if (firstConnection)
            {
                Send(new IdentifyPayload
                {
                    Data = identify
                });

                firstConnection = false;
            }
        }

        public async void Heartbeat(JsonHello hello, CancellationToken token)
        {
            try
            {
                while (true)
                {
                    await Task.Delay(hello.HeartbeatInterval, token);
                    Send(new HeartbeatPayload
                    {
                        Data = sequenceNumber
                    });
                }
            }
            catch (Exception)
            {
                tokenSource.Dispose();
                return;
            }
        }

        public async Task<bool> Open()
        {
            return await socket.OpenAsync();
        }

        public void Send(object obj)
        {
            string json = JsonConvert.SerializeObject(obj);

            Console.WriteLine(json);
            Console.WriteLine();

            byte[] payload = Encoding.UTF8.GetBytes(json);
            socket.Send(payload, 0, payload.Length);
        }

        public void Dispose()
        {
            socket.Dispose();
            if (tokenSource != null)
            {
                tokenSource.Dispose();
            }
        }
    }
}
