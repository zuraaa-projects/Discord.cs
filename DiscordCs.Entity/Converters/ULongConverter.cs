﻿using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FarDragi.DiscordCs.Entity.Converters
{
    public class ULongConverter : JsonConverter<ulong>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(ulong) == typeToConvert;
        }

        public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (ulong.TryParse(reader.GetString(), out ulong result))
            {
                return result;
            }

            return 0;
        }

        public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
