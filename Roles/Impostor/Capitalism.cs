﻿using System;
using System.Collections.Generic;
using static TOHE.Options;

namespace TOHE.Roles.Impostor
{
    internal class Capitalism : RoleBase
    {
        public static Dictionary<byte, int> CapitalismAddTask = [];
        public static Dictionary<byte, int> CapitalismAssignTask = [];
        public static bool On;
        public override bool IsEnable => On;

        public static void SetupCustomOption()
        {
            SetupRoleOptions(16600, TabGroup.OtherRoles, CustomRoles.Capitalism);
            CapitalismSkillCooldown = FloatOptionItem.Create(16610, "CapitalismSkillCooldown", new(0f, 60f, 1f), 10f, TabGroup.OtherRoles, false)
                .SetParent(CustomRoleSpawnChances[CustomRoles.Capitalism])
                .SetValueFormat(OptionFormat.Seconds);
            CapitalismKillCooldown = FloatOptionItem.Create(16611, "KillCooldown", new(2.5f, 60f, 2.5f), 25f, TabGroup.OtherRoles, false)
                .SetParent(CustomRoleSpawnChances[CustomRoles.Capitalism])
                .SetValueFormat(OptionFormat.Seconds);
        }

        public override void Add(byte playerId)
        {
            On = true;
        }

        public override void Init()
        {
            On = false;
        }

        public override void SetButtonTexts(HudManager hud, byte id)
        {
            hud.KillButton?.OverrideText(Translator.GetString("CapitalismButtonText"));
        }

        public override void SetKillCooldown(byte id)
        {
            Main.AllPlayerKillCooldown[id] = Options.CapitalismKillCooldown.GetFloat();
        }

        public override bool OnCheckMurder(PlayerControl killer, PlayerControl target)
        {
            return killer.CheckDoubleTrigger(target, () =>
            {
                CapitalismAddTask.TryAdd(target.PlayerId, 0);
                CapitalismAddTask[target.PlayerId]++;
                CapitalismAssignTask.TryAdd(target.PlayerId, 0);
                CapitalismAssignTask[target.PlayerId]++;
                Logger.Info($"{killer.GetRealName()} added a task for：{target.GetRealName()}", "Capitalism Add Task");
                //killer.RpcGuardAndKill(killer);
                killer.SetKillCooldown(Options.CapitalismSkillCooldown.GetFloat());
            });
        }

        public static bool AddTaskForPlayer(PlayerControl player)
        {
            if (CapitalismAddTask.TryGetValue(player.PlayerId, out var amount))
            {
                var taskState = player.GetTaskState();
                taskState.AllTasksCount += amount;
                CapitalismAddTask.Remove(player.PlayerId);
                taskState.CompletedTasksCount++;
                GameData.Instance.RpcSetTasks(player.PlayerId, Array.Empty<byte>()); // Redistribute tasks
                player.SyncSettings();
                Utils.NotifyRoles(SpecifySeer: player, SpecifyTarget: player);
                return false;
            }

            return true;
        }
    }
}
