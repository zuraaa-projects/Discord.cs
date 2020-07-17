﻿namespace FarDragi.DiscordCs.Core.Base.Models.Base
{
    public class DiscordGuild
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public ulong OwnerId { get; set; }
        public string Region { get; set; }
        public string Splash { get; set; }
        public string DiscoverySplash { get; set; }
        public bool IsUnavailable { get; set; }
    }
}