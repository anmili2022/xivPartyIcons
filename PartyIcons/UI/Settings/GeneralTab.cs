using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using PartyIcons.UI.Utils;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Numerics;

namespace PartyIcons.UI.Settings;

public sealed class GeneralTab
{
    private readonly Notice _notice = new();
    private readonly FlashingText _flashingText = new();

    public void Draw()
    {
        ImGui.Dummy(new Vector2(0, 2f));

        using (ImRaii.PushColor(ImGuiCol.CheckMark, 0xFF888888)) {
            var usePriorityIcons = true;
            ImGui.Checkbox("##usePriorityIcons", ref usePriorityIcons);
            ImGui.SameLine();
            ImGui.Text("优先显示状态图标");
            using (ImRaii.PushIndent())
            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudOrange)) {
                ImGui.TextWrapped(
                    "注意：优先状态图标现在可以通过'外观'选项卡中的'交换样式'选项为每种姓名板类型进行配置。您还可以在'状态图标'选项卡中配置哪些图标被认为足够重要而需要优先显示。");
            }
            ImGui.Dummy(new Vector2(0, 3));
        }

        var testingMode = Plugin.Settings.TestingMode;
        if (ImGui.Checkbox("##testingMode", ref testingMode)) {
            Plugin.Settings.TestingMode = testingMode;
            Plugin.Settings.Save();
        }

        ImGui.SameLine();
        using (_flashingText.PushColor(Plugin.Settings.TestingMode)) {
            ImGui.Text("启用测试模式");
        }
        ImGuiComponents.HelpMarker("将设置应用于任何玩家，而不仅仅是队伍中的玩家。");

        var chatContentMessage = Plugin.Settings.ChatContentMessage;

        if (ImGui.Checkbox("##chatmessage", ref chatContentMessage)) {
            Plugin.Settings.ChatContentMessage = chatContentMessage;
            Plugin.Settings.Save();
        }

        ImGui.SameLine();
        ImGui.Text("进入副本时显示聊天消息");
        ImGuiComponents.HelpMarker("可用于在完全加载前确定副本类型。");

        ImGuiExt.Spacer(10);
        if (ImGuiExt.ButtonEnabledWhen(ImGui.GetIO().KeyCtrl, "再次显示升级指南")) {
            UpgradeGuideTab.ForceRedisplay = true;
        }
        ImGuiExt.HoverTooltip("按住Control键允许点击");

        _notice.DisplayNotice();
    }
}

public sealed class Notice
{
    private string? _noticeString;
    private string? _noticeUrl;

    public Notice()
    {
        DownloadAndParseNotice();
    }

    private void DownloadAndParseNotice()
    {
        using var httpClient = new HttpClient();
        try {
            var strArray = httpClient.GetStringAsync("https://shdwp.github.io/ukraine/xiv_notice.txt").Result.Split('|');

            if (strArray.Length > 0) {
                _noticeString = strArray[0].Replace("\n", "\n\n");
            }

            if (strArray.Length < 2) {
                return;
            }

            _noticeUrl = strArray[1];

            if (!(_noticeUrl.StartsWith("http://") || _noticeUrl.StartsWith("https://"))) {
                Service.Log.Warning($"Received invalid noticeUrl {_noticeUrl}, ignoring");
                _noticeUrl = null;
            }
        }
        catch (Exception) {
            // ignored
        }
    }

    public void DisplayNotice()
    {
        if (_noticeString == null)
            return;

        ImGui.Dummy(new Vector2(0.0f, 15f));

        using var col = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DPSRed);

        ImGui.TextWrapped(_noticeString);

        if (_noticeUrl != null && ImGui.Button(_noticeUrl)) {
            try {
                Process.Start(new ProcessStartInfo
                {
                    FileName = _noticeUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception) {
                // ignored
            }
        }
    }
}