﻿using FarDragi.DiscordCs.Entities.ChannelModels;
using FarDragi.DiscordCs.Entities.EmojiModels;
using FarDragi.DiscordCs.Entities.MemberModels;
using FarDragi.DiscordCs.Entities.PresenceModels;
using FarDragi.DiscordCs.Entities.RoleModels;
using FarDragi.DiscordCs.Entities.VoiceModels;
using System;

namespace FarDragi.DiscordCs.Entities.GuildModels
{
    /// <summary>
    /// https://discord.com/developers/docs/resources/guild#guild-object-guild-structure
    /// </summary>
    public class Guild
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public string Icon { get; set; }
        public string IconHash { get; set; }
        public string Splash { get; set; }
        public string DiscoverySplash { get; set; }
        public bool Owner { get; set; }
        public ulong OwnerId { get; set; }
        public string Permissions { get; set; }
        public string Region { get; set; }
        public ulong? AfkChannelId { get; set; }
        public int AfkTimeout { get; set; }
        public bool WidgetEnabled { get; set; }
        public ulong? WidgetChannelId { get; set; }
        public GuildVerificationLevel VerificationLevel { get; set; }
        public GuildMessageNotificationLevel DefaultMessageNotifications { get; set; }
        public GuildExplicitContentFilterLevel ExplicitContentFilter { get; set; }
        public Role[] Roles { get; set; }
        public Emoji[] Emojis { get; set; }
        public string[] Features { get; set; }
        public GuildMfaLevel MfaLevel { get; set; }
        public ulong? ApplicationId { get; set; }
        public ulong SystemChannelId { get; set; }
        public GuildSystemChannelFlags SystemChannelFlags { get; set; }
        public ulong? RulesChannelId { get; set; }
        public DateTime? JoinedAt { get; set; }
        public bool Large { get; set; }
        public bool Unavailable { get; set; }
        public int MemberCount { get; set; }
        public Voice[] Voices { get; set; }
        public Member[] Members { get; set; }
        public Channel[] Channels { get; set; }
        public Presence[] Presences { get; set; }
        public int MaxPresences { get; set; }
        public int? MaxMembers { get; set; }
        public string VanityUrlCode { get; set; }
        public string Description { get; set; }
        public string Banner { get; set; }
        public GuildPremiumTier PremiumTier { get; set; }
        public int? PremiumSubscriptionCount { get; set; }
        public string PreferredLocale { get; set; }
        public ulong PublicUpdatesChannelId { get; set; }
        public int? MaxVideoChannelUsers { get; set; }
        public int? ApproximateMemberCount { get; set; }
        public int? ApproximatePresenceCount { get; set; }
    }
}