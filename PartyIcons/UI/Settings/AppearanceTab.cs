using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using PartyIcons.Configuration;
using PartyIcons.Runtime;
using PartyIcons.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Action = System.Action;

namespace PartyIcons.UI.Settings;

public sealed class AppearanceTab
{
    private NameplateMode _createMode = NameplateMode.SmallJobIcon;

    public void Draw()
    {
        ImGuiExt.Spacer(2);

        ImGui.TextDisabled("配置姓名板外观");

        ImGuiExt.SectionHeader("预设");

        List<Action> actions = [];

        DrawDisplayConfig(Plugin.Settings.DisplayConfigs.SmallJobIcon, ref actions);
        DrawDisplayConfig(Plugin.Settings.DisplayConfigs.SmallJobIconAndRole, ref actions);
        DrawDisplayConfig(Plugin.Settings.DisplayConfigs.BigJobIcon, ref actions);
        DrawDisplayConfig(Plugin.Settings.DisplayConfigs.BigJobIconAndPartySlot, ref actions);
        DrawDisplayConfig(Plugin.Settings.DisplayConfigs.RoleLetters, ref actions);

        ImGuiExt.SectionHeader("用户创建的");

        var modes = Enum.GetValues<NameplateMode>()
            .Where(v => v is not (NameplateMode.Default or NameplateMode.Hide))
            .ToList();

        ImGuiExt.SetComboWidth(modes.Select(UiNames.GetName));

        using (var combo = ImRaii.Combo("##newDisplay", UiNames.GetName(_createMode))) {
            if (combo) {
                foreach (var mode in modes) {
                    if (ImGui.Selectable(UiNames.GetName(mode), mode == _createMode)) {
                        Service.Log.Info($"set to {mode}");
                        _createMode = mode;
                    }
                }
            }
        }

        ImGui.SameLine();
        if (ImGui.Button("创建新预设")) {
            Plugin.Settings.DisplayConfigs.Custom.Add(
                new DisplayConfig($"自定义 {Plugin.Settings.DisplayConfigs.Custom.Count + 1}", _createMode));
            Plugin.Settings.Save();
        }

        ImGuiExt.Spacer(2);

        foreach (var statusConfig in Plugin.Settings.DisplayConfigs.Custom) {
            DrawDisplayConfig(statusConfig, ref actions);
        }

        foreach (var action in actions) {
            action();
        }
    }

    private static void DrawDisplayConfig(DisplayConfig config, ref List<Action> actions)
    {
        using var id = ImRaii.PushId($"display@{config.Preset}@{config.Id}");

        if (!ImGui.CollapsingHeader($"{UiNames.GetName(config)}###statusHeader@{config.Preset}@{config.Id}"))
            return;

        using var indent = ImRaii.PushIndent();

        if (config.Preset == DisplayPreset.Custom) {
            ImGui.TextDisabled("用户创建的");
            // ImGui.TextDisabled("基于: ");
            // ImGui.SameLine();
            ImGui.TextUnformatted(UiNames.GetName(config.Mode));

            var name = config.Name ?? "";
            if (ImGui.InputText("名称##rename", ref name, 100, ImGuiInputTextFlags.EnterReturnsTrue)) {
                actions.Add(() =>
                {
                    config.Name = name.Replace("%", "");
                    Plugin.Settings.Save();
                });
            }

            ImGui.Dummy(new Vector2(0, 6));
        }

        ImGui.TextDisabled("外观");

        ImGuiExt.DrawIconSetCombo("图标集", true, () => config.IconSetId, iconSetId =>
        {
            config.IconSetId = iconSetId;
            Plugin.Settings.Save();
        });

        var scale = config.Scale;
        if (ImGui.SliderFloat("缩放", ref scale, 0.3f, 3f)) {
            config.Scale = Math.Clamp(scale, 0.1f, 10f);
            Plugin.Settings.Save();
        }

        ImGuiComponents.HelpMarker("按住Control键并点击滑块可以输入精确数值");

        using (var combo = ImRaii.Combo("交换样式", config.SwapStyle.ToString())) {
            if (combo) {
                foreach (var style in Enum.GetValues<StatusSwapStyle>()) {
                    if (ImGui.Selectable(style.ToString(), style == config.SwapStyle)) {
                        config.SwapStyle = style;
                        Plugin.Settings.Save();
                    }
                }
            }
        }

        ImGuiComponents.HelpMarker(
            """
            决定如何为设置为"重要"的状态执行图标交换：
            - "None"不执行任何操作
            - "Swap"将交换状态图标和职业图标的位置
            - "Replace"将状态图标移动到职业图标位置，保持状态图标位置为空
            """);

        if (config.Mode is NameplateMode.SmallJobIconAndRole or NameplateMode.RoleLetters) {
            using var combo = ImRaii.Combo("职能显示样式", UiNames.GetName(config.RoleDisplayStyle));
            if (combo) {
                foreach (var style in Enum.GetValues<RoleDisplayStyle>().Where(r => r != RoleDisplayStyle.None)) {
                    if (ImGui.Selectable(UiNames.GetName(style), style == config.RoleDisplayStyle)) {
                        config.RoleDisplayStyle = style;
                        Plugin.Settings.Save();
                    }
                }
            }
        }

        ImGuiExt.Spacer(6);
        ImGui.TextDisabled("职业图标");
        using (ImRaii.PushId("jobIcon")) {
            DrawJobIcon(() => config.JobIcon, icon => config.JobIcon = icon);
        }

        ImGui.TextDisabled("状态图标");
        using (ImRaii.PushId("statusIcon")) {
            DrawJobIcon(() => config.StatusIcon, icon => config.StatusIcon = icon);
        }

        ImGuiExt.Spacer(6);
        ImGui.TextDisabled("按区域显示状态图标");
        foreach (var zoneType in Enum.GetValues<ZoneType>()) {
            DrawStatusSelector(config, zoneType);
        }

        ImGuiExt.Spacer(6);
        ImGui.TextDisabled("其他操作");
        if (ImGuiExt.ButtonEnabledWhen(ImGui.GetIO().KeyCtrl, "重置为默认值")) {
            config.Reset();
            Plugin.Settings.Save();
        }

        ImGuiExt.HoverTooltip("按住Control键允许重置");

        if (config.Preset == DisplayPreset.Custom) {
            ImGui.SameLine();
            if (ImGuiExt.ButtonEnabledWhen(ImGui.GetIO().KeyCtrl, "删除")) {
                actions.Add(() =>
                {
                    Plugin.Settings.DisplaySelectors.RemoveSelectors(config);
                    Plugin.Settings.DisplayConfigs.Custom.RemoveAll(c => c.Id == config.Id);
                    Plugin.Settings.Save();
                });
            }

            ImGuiExt.HoverTooltip("按住Control键允许删除");
        }

        ImGuiExt.Spacer(3);
    }

    private static void DrawJobIcon(Func<IconCustomizeConfig> getter, Action<IconCustomizeConfig> setter)
    {
        var icon = getter();

        var show = icon.Show;
        if (ImGui.Checkbox("显示", ref show)) {
            setter(icon with { Show = show });
            Plugin.Settings.Save();
        }

        var scale = icon.Scale;
        if (ImGui.SliderFloat("缩放", ref scale, 0.3f, 3f)) {
            setter(icon with { Scale = Math.Clamp(scale, 0.1f, 10f) });
            Plugin.Settings.Save();
        }

        ImGuiComponents.HelpMarker("按住Control键并点击滑块可以输入精确数值");

        int[] pos = [icon.OffsetX, icon.OffsetY];
        if (ImGui.SliderInt("X/Y偏移", ref pos[0], -50, 50)) {
            var x = (short)Math.Clamp(pos[0], -1000, 1000);
            var y = (short)Math.Clamp(pos[1], -1000, 1000);
            setter(icon with { OffsetX = x, OffsetY = y });
            Plugin.Settings.Save();
        }

        ImGuiComponents.HelpMarker("按住Control键并点击滑块可以输入精确数值");
    }

    private static void DrawStatusSelector(DisplayConfig config, ZoneType zoneType)
    {
        var currentSelector = config.StatusSelectors[zoneType];
        ImGuiExt.SetComboWidth(Plugin.Settings.StatusConfigs.Selectors.Select(UiNames.GetName));
        using var combo = ImRaii.Combo($"{UiNames.GetName(zoneType)}##zoneSelector@{zoneType}",
            UiNames.GetName(currentSelector));
        if (!combo) return;

        foreach (var selector in Plugin.Settings.StatusConfigs.Selectors) {
            using var col = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen,
                selector.Preset == StatusPreset.Custom);
            if (ImGui.Selectable(UiNames.GetName(selector), currentSelector == selector)) {
                config.StatusSelectors[zoneType] = selector;
                Plugin.Settings.Save();
            }
        }
    }
}