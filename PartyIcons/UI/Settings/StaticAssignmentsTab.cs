using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using PartyIcons.Entities;
using PartyIcons.UI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PartyIcons.UI.Settings;

public sealed class StaticAssignmentsTab
{
    private string _occupationNewName = "角色名称@服务器";
    private RoleId _occupationNewRole = RoleId.Undefined;

    public void Draw()
    {
        ImGui.Dummy(new Vector2(0, 2f));
        
        var easternNamingConvention = Plugin.Settings.EasternNamingConvention;

        if (ImGui.Checkbox("##easteannaming", ref easternNamingConvention))
        {
            Plugin.Settings.EasternNamingConvention = easternNamingConvention;
            Plugin.Settings.Save();
        }

        ImGui.SameLine();
        ImGui.Text("使用东方职能命名约定");
        ImGuiComponents.HelpMarker("使用日本数据中心的职能命名约定（MT ST D1-D4 H1-2）。");

        var displayRoleInPartyList = Plugin.Settings.DisplayRoleInPartyList;

        if (ImGui.Checkbox("##displayrolesinpartylist", ref displayRoleInPartyList))
        {
            Plugin.Settings.DisplayRoleInPartyList = displayRoleInPartyList;
            Plugin.Settings.Save();
        }

        ImGui.SameLine();
        ImGui.Text("在小队列表中用职能替换队伍编号");
        ImGuiExt.ImGuiHelpTooltip(
            "仅在姓名板设置为'职能字母'或'小职业图标、职能和名称'时有效。");

        var useContextMenu = Plugin.Settings.UseContextMenu;
        
        if (ImGui.Checkbox("##useContextMenu", ref useContextMenu))
        {
            Plugin.Settings.UseContextMenu = useContextMenu;
            Plugin.Settings.Save();
        }

        ImGui.SameLine();
        ImGui.Text("添加上下文菜单命令以分配职能");
        ImGuiComponents.HelpMarker("添加上下文菜单命令以为玩家分配职能。在适用情况下，还会添加交换职能和使用建议职能的命令。");

        var useContextMenuStatic = Plugin.Settings.UseContextMenuStatic;

        if (ImGui.Checkbox("##useContextMenuStatic", ref useContextMenuStatic))
        {
            Plugin.Settings.UseContextMenuStatic = useContextMenuStatic;
            Plugin.Settings.Save();
        }

        ImGui.SameLine();
        ImGui.Text("添加上下文菜单命令以保存固定职能");

        var useContextMenuSubmenu = Plugin.Settings.UseContextMenuSubmenu;

        if (ImGui.Checkbox("##useContextMenuSubmenu", ref useContextMenuSubmenu))
        {
            Plugin.Settings.UseContextMenuSubmenu = useContextMenuSubmenu;
            Plugin.Settings.Save();
        }

        ImGui.SameLine();
        ImGui.Text("将上下文菜单命令（如果启用）放在专用子菜单中");

        var assignFromChat = Plugin.Settings.AssignFromChat;

        if (ImGui.Checkbox("##assignFromChat", ref assignFromChat))
        {
            Plugin.Settings.AssignFromChat = assignFromChat;
            Plugin.Settings.Save();
        }

        ImGui.SameLine();
        ImGui.Text("允许小队成员通过小队聊天自行分配职能");
        ImGuiComponents.HelpMarker("允许小队成员为自己分配职能，例如在小队聊天中输入'h1'将为该玩家分配H1职能。");
        
        ImGui.Dummy(new Vector2(0, 2f));
        ImGui.PushStyleColor(0, ImGuiHelpers.DefaultColorPalette()[0]);
        ImGui.Text("固定职能");
        ImGui.PopStyleColor();
        ImGui.Separator();
        ImGui.Dummy(new Vector2(0, 2f));
        {
            ImGui.PushStyleColor(0, ImGuiColors.ParsedGrey);
            {
                ImGui.TextWrapped(
                    "名称应包含服务器名，用@分隔。请注意，如果玩家的职业不适合所分配的职能，分配将被忽略！");
                ImGui.Dummy(new Vector2(0f, 25f));
            }
            ImGui.PopStyleColor();
        }
        
        ImGui.SetCursorPosY(ImGui.GetCursorPos().Y - 22f);
        foreach (var kv in new Dictionary<string, RoleId>(Plugin.Settings.StaticAssignments))
        {
            if (ImGui.Button("x##remove_occupation_" + kv.Key))
            {
                Plugin.Settings.StaticAssignments.Remove(kv.Key);
                Plugin.Settings.Save();

                continue;
            }

            ImGui.SameLine();
            ImGuiExt.SetComboWidth(Enum.GetValues<RoleId>().Select(x => Plugin.PlayerStylesheet.GetRoleName(x)));

            if (ImGui.BeginCombo("##role_combo_" + kv.Key,
                    Plugin.PlayerStylesheet.GetRoleName(Plugin.Settings.StaticAssignments[kv.Key])))
            {
                foreach (var roleId in Enum.GetValues<RoleId>())
                {
                    if (ImGui.Selectable(Plugin.PlayerStylesheet.GetRoleName(roleId) + "##role_combo_option_" + kv.Key + "_" +
                                         roleId))
                    {
                        Plugin.Settings.StaticAssignments[kv.Key] = roleId;
                        Plugin.Settings.Save();
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.SameLine();
            ImGui.Text(kv.Key);
        }

        if (ImGui.Button("+##add_occupation"))
        {
            Plugin.Settings.StaticAssignments[_occupationNewName] = _occupationNewRole;
            Plugin.Settings.Save();
        }

        ImGui.SameLine();
        ImGuiExt.SetComboWidth(Enum.GetValues<RoleId>().Select(x => Plugin.PlayerStylesheet.GetRoleName(x)));

        if (ImGui.BeginCombo("##new_role_combo", Plugin.PlayerStylesheet.GetRoleName(_occupationNewRole)))
        {
            foreach (var roleId in Enum.GetValues<RoleId>())
            {
                if (ImGui.Selectable(Plugin.PlayerStylesheet.GetRoleName(roleId) + "##new_role_combo_option_" + "_" + roleId))
                {
                    _occupationNewRole = roleId;
                }
            }

            ImGui.EndCombo();
        }

        ImGui.SameLine();
        ImGui.InputText("##new_role_name", ref _occupationNewName, 64);
        
        ImGui.SetCursorPosY(ImGui.GetCursorPos().Y + 22f);

    }
}