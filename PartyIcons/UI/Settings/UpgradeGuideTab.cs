using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using PartyIcons.Configuration;
using PartyIcons.Runtime;
using PartyIcons.UI.Utils;
using System.Linq;
using System.Numerics;

namespace PartyIcons.UI.Settings;

public static class UpgradeGuideTab
{
    public static bool ForceRedisplay { get; set; }

    public static void Draw()
    {
        var buttonSize = new Vector2(100f, 80f) * ImGuiHelpers.GlobalScale;

        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.ParsedGreen)) {
            ImGui.TextWrapped("队伍图标升级指南（1.2版本）");
        }

        ImGui.TextWrapped("Party Icons的黎明之追更新带来了状态图标显示方式的一些变化。");
        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudGrey2)) {
            ImGui.TextWrapped("（状态图标是指断线、观看过场动画、等待任务、指导者、新冒险者（初学者图标）等在线状态的图标。）");
        }
        ImGui.TextWrapped("从此版本开始，现在可以同时显示职业图标和状态图标，并为每种姓名板显示类型自定义此设置。");

        ImGuiExt.Spacer(8);
        using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudYellow)) {
            ImGui.TextWrapped("您希望如何显示状态图标？");
        }
        ImGuiExt.Spacer(8);

        if (ImGui.Button("使用新的\n    默认设置", buttonSize)) {
            UseNewDefaults();
        }
        ImGui.SameLine();
        ImGui.TextWrapped("在副本中，某些重要状态会与职业图标交换位置，但其他状态大多隐藏。在野外，大多数状态会在其自己的状态图标位置显示。");
        ImGui.Separator();

        if (ImGui.Button("复制旧的\n  优先级\n      系统", buttonSize)) {
            ReplicatePriority();
        }
        ImGui.SameLine();
        ImGui.TextWrapped("某些重要的状态图标（如断线、暂离或观看过场动画）在适当时会完全替换职业图标，但其他状态图标则会隐藏。");
        ImGui.Separator();

        if (ImGui.Button(" 完全不显示\n状态图标", buttonSize)) {
            NoIcons();
        }
        ImGui.SameLine();
        ImGui.TextWrapped("仅显示职业图标。");
        ImGui.Separator();

        if (ForceRedisplay) {
            if (ImGui.Button("取消", buttonSize)) {
                Cancel();
            }
            ImGui.SameLine();
            ImGui.TextWrapped("不做任何更改。");
            ImGui.Separator();
        }

        ImGui.TextWrapped("请选择上述选项之一以继续。您可以从'常规'选项卡再次打开此窗口，或从'外观'和'状态图标'选项卡更详细地自定义状态图标可见性。");
    }

    private static void UseNewDefaults()
    {
        foreach (var config in Plugin.Settings.DisplayConfigs.Configs.Where(c => c.Preset != DisplayPreset.Custom)) {
            config.StatusIcon.Show = true;
            config.SwapStyle = StatusSwapStyle.Swap;
            config.StatusSelectors[ZoneType.Overworld] = new StatusSelector(StatusPreset.Overworld);
        }
        Plugin.Settings.SelectorsDialogComplete = true;
        Plugin.Settings.Save();
        ForceRedisplay = false;

    }

    private static void ReplicatePriority()
    {
        foreach (var config in Plugin.Settings.DisplayConfigs.Configs.Where(c => c.Preset != DisplayPreset.Custom)) {
            config.StatusIcon.Show = true;
            config.SwapStyle = StatusSwapStyle.Replace;
            config.StatusSelectors[ZoneType.Overworld] = new StatusSelector(StatusPreset.OverworldLegacy);
        }
        Plugin.Settings.SelectorsDialogComplete = true;
        Plugin.Settings.Save();
        ForceRedisplay = false;
    }

    private static void NoIcons()
    {
        foreach (var config in Plugin.Settings.DisplayConfigs.Configs.Where(c => c.Preset != DisplayPreset.Custom)) {
            config.StatusIcon.Show = false;
        }
        Plugin.Settings.SelectorsDialogComplete = true;
        Plugin.Settings.Save();
        ForceRedisplay = false;
    }

    private static void Cancel()
    {
        ForceRedisplay = false;
    }
}