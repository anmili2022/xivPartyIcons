using System;
using PartyIcons.Configuration;
using PartyIcons.Runtime;

namespace PartyIcons.UI.Utils;

public static class UiNames
{
    public static string GetName(NameplateMode mode)
    {
        return mode switch
        {
            NameplateMode.Default => "游戏默认",
            NameplateMode.Hide => "隐藏",
            NameplateMode.BigJobIcon => "大职业图标",
            NameplateMode.SmallJobIcon => "小职业图标和名称",
            NameplateMode.SmallJobIconAndRole => "小职业图标、职能和名称",
            NameplateMode.BigJobIconAndPartySlot => "大职业图标和队伍编号",
            NameplateMode.RoleLetters => "职能字母",
            _ => $"未知 ({(int)mode}/{mode.ToString()})"
        };
    }

    public static string GetName(ZoneType zoneType)
    {
        return zoneType switch
        {
            ZoneType.Overworld => "野外",
            ZoneType.Dungeon => "4人迷宫",
            ZoneType.Raid => "8人副本",
            ZoneType.AllianceRaid => "团队副本",
            ZoneType.ChaoticRaid => "诛灭战",
            ZoneType.FieldOperation => "特殊场景探索",
            _ => $"未知 ({(int)zoneType}/{zoneType.ToString()})"
        };
    }

    public static string GetName(StatusConfig config)
    {
        return config.Preset switch
        {
            StatusPreset.Custom => config.Name ?? "<未命名>",
            StatusPreset.Overworld => "野外",
            StatusPreset.Instances => "副本",
            StatusPreset.FieldOperations => "特殊场景探索",
            StatusPreset.OverworldLegacy => "野外（传统）",
            _ => config.Preset + "/" + config.Name + "/" + config.Id
        };
    }

    public static string GetName(StatusSelector selector)
    {
        return GetName(Plugin.Settings.GetStatusConfig(selector));
    }

    public static string GetName(DisplayConfig config)
    {
        if (config.Preset == DisplayPreset.Custom) {
            return $"{GetName(config.Mode)} ({config.Name})";
        }

        return GetName(config.Mode);
    }

    public static string GetName(DisplaySelector selector)
    {
        return GetName(Plugin.Settings.GetDisplayConfig(selector));
    }

    public static string GetName(IconSetId id)
    {
        return id switch
        {
            IconSetId.EmbossedFramed => "带框架，职能着色",
            IconSetId.EmbossedFramedSmall => "带框架，职能着色（小）",
            IconSetId.Gradient => "渐变，职能着色",
            IconSetId.Glowing => "发光",
            IconSetId.Embossed => "浮雕",
            IconSetId.Inherit => "<使用全局设置>",
            _ => id.ToString()
        };
    }

    public static string GetName(ChatMode mode)
    {
        return mode switch
        {
            ChatMode.GameDefault => "游戏默认",
            ChatMode.Role => "职能",
            ChatMode.Job => "职业缩写",
            _ => throw new ArgumentException($"未知聊天模式 {mode}")
        };
    }

    public static string GetName(RoleDisplayStyle style)
    {
        return style switch
        {
            RoleDisplayStyle.None => "无",
            RoleDisplayStyle.Role => "职能",
            RoleDisplayStyle.PartyNumber => "队伍编号",
            _ => throw new ArgumentException($"未知职能显示样式 {style}")
        };
    }
}