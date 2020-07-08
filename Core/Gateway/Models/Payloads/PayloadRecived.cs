﻿using FarDragi.DragiCordApi.Core.Gateway.Codes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FarDragi.DragiCordApi.Core.Gateway.Models.Payloads
{
    internal sealed class PayloadRecived<TData>
    {
        [JsonProperty("t")]
        public string Type { get; set; }
        [JsonProperty("s")]
        public ulong Session { get; set; }
        [JsonProperty("op")]
        public GatewayOpcode Opcode { get; set; }
        [JsonProperty("d")]
        public TData Data { get; set; }
    }
}
