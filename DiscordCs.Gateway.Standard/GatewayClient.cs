﻿using FarDragi.DiscordCs.Caching;
using FarDragi.DiscordCs.Entity.Converters;
using FarDragi.DiscordCs.Entity.Interfaces;
using FarDragi.DiscordCs.Entity.Models.HelloModels;
using FarDragi.DiscordCs.Entity.Models.IdentifyModels;
using FarDragi.DiscordCs.Entity.Models.PayloadModels;
using FarDragi.DiscordCs.Entity.Models.ResumeModels;
using FarDragi.DiscordCs.Gateway.Standard.Functions;
using FarDragi.DiscordCs.Logging;
using FarDragi.DiscordCs.Rest;
using SuperSocket.ClientEngine;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WebSocket4Net;

namespace FarDragi.DiscordCs.Gateway.Standard
{
    public class GatewayClient : IGatewayClient
    {
        private readonly IGatewayContext _gatewayContext;
        private readonly Identify _identify;
        private readonly GatewayConfig _config;
        private readonly ILogger _logger;
        private readonly Decompressor _decompressor;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly System.Diagnostics.Stopwatch _stopwatch;

        private WebSocket _socket;
        private CancellationTokenSource _tokenSource;
        private int _sequenceNumber;
        private bool _firstConnection;
        private long _ping;

        public GatewayClient(
            IGatewayContext gatewayContext,
            Identify identify,
            GatewayConfig config,
            ILogger logger,
            JsonSerializerOptions serializerOptions)
        {
            _gatewayContext = gatewayContext;
            _identify = identify;
            _config = config;
            _logger = logger;
            _jsonSerializerOptions = serializerOptions;
            _firstConnection = true;
            _decompressor = new Decompressor();
            _stopwatch = new System.Diagnostics.Stopwatch();
        }

        #region Get/Set

        public long Ping { get => _ping; }
        public string SessionId { get; set; }

        #endregion

        #region Events

        private void InitSocket()
        {
            _socket = new WebSocket(_config.GetUrl());
            _socket.Closed += Socket_Closed;
            _socket.Error += Socket_Error;
            _socket.DataReceived += Socket_DataReceived;
        }

        private async void Socket_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (_decompressor.TryDecompress(e.Data, out string json))
            {
                Payload<JsonElement> payload = JsonSerializer.Deserialize<Payload<JsonElement>>(json, _jsonSerializerOptions);

                _sequenceNumber = payload.SequenceNumber ?? _sequenceNumber;

                switch (payload.OpCode)
                {
                    case PayloadOpCode.Dispatch:
                        Received(payload, json);
                        break;
                    case PayloadOpCode.Hello:
                        Hello hello = payload.Data.ToObject<Hello>(_jsonSerializerOptions);
                        _tokenSource = new CancellationTokenSource();
                        await Heartbeat(hello, _tokenSource.Token).ConfigureAwait(false);
                        break;
                    case PayloadOpCode.HeartbeatACK:
                        _stopwatch.Stop();
                        _ping = _stopwatch.ElapsedMilliseconds;
                        _logger.Log(LoggingLevel.Info, $"Received heartbeat {_ping}ms");
                        _stopwatch.Reset();
                        break;
                    case PayloadOpCode.InvalidSession:
                        _firstConnection = true;
                        _socket.Close();
                        break;
                    case PayloadOpCode.Reconnect:
                        break;
                    default:
                        break;
                }

                _logger.Log(LoggingLevel.Verbose, json);
            }
        }

        private void Socket_Error(object sender, ErrorEventArgs e)
        {
            _socket.Close();
        }

        private void Socket_Closed(object sender, EventArgs e)
        {
            if (e is ClosedEventArgs args)
            {
                if (args.Code == 4004)
                {
                    _logger.Log(LoggingLevel.Severity, $"[Websocket] Code: {args.Code} Reason: {args.Reason}");
                    return;
                }
                else
                {
                    _logger.Log(LoggingLevel.Warning, $"[Websocket] Code: {args.Code} Reason: {args.Reason}");

                }

                _socket.Dispose();
                _tokenSource.Cancel();
                InitSocket();
                _socket.Open();
            }
        }

        #endregion

        #region Recive/Send

        public void Received(Payload<JsonElement> payload, string json)
        {
            _logger.Log(LoggingLevel.Info, $"Received {payload.Event}");
            _gatewayContext.OnReceivedEvent(this, payload, json, _jsonSerializerOptions);
        }

        public void Send(object obj)
        {
            byte[] payload = JsonSerializer.SerializeToUtf8Bytes(obj, _jsonSerializerOptions);
            _socket.Send(payload, 0, payload.Length);
        }

        private async Task Heartbeat(Hello hello, CancellationToken token)
        {
            try
            {
                if (_firstConnection)
                {
                    Send(new PayloadIdentify
                    {
                        Data = _identify
                    });

                    _firstConnection = false;
                }
                else
                {
                    Send(new PayloadResume
                    {
                        Data = new Resume
                        {
                            SequenceNumber = _sequenceNumber,
                            SessionId = SessionId,
                            Token = _identify.Token
                        }
                    });
                }

                while (true)
                {
                    await Task.Delay(hello.HeartbeatInterval, token);

                    _stopwatch.Start();
                    Send(new PayloadHeartbeat
                    {
                        Data = _sequenceNumber
                    });
                    _logger.Log(LoggingLevel.Info, "Send heartbeat");
                }
            }
            catch (Exception)
            {
                _tokenSource.Dispose();
            }
        }

        #endregion

        #region On/Off

        public Task Open()
        {
            _logger.Log(LoggingLevel.Info, $"Start shard {_identify.Shard[0]}");
            InitSocket();
            _socket.Open();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _socket?.Dispose();
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }

        #endregion
    }
}
