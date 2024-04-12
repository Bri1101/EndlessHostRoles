﻿using System.Collections.Generic;

namespace EHR.Roles.AddOns.GhostRoles
{
    internal class Bloodmoon : IGhostRole, ISettingHolder
    {
        public Team Team => Team.Impostor | Team.Neutral;
        public int Cooldown => Duration.GetInt() + 30;

        private static OptionItem Duration;

        private static readonly Dictionary<byte, long> ScheduledDeaths = [];

        public void SetupCustomOption()
        {
            Options.SetupRoleOptions(649400, TabGroup.OtherRoles, CustomRoles.Bloodmoon);
            Duration = IntegerOptionItem.Create(649402, "Bloodmoon.Duration", new(0, 60, 1), 15, TabGroup.OtherRoles)
                .SetParent(Options.CustomRoleSpawnChances[CustomRoles.Bloodmoon])
                .SetValueFormat(OptionFormat.Seconds);
        }

        public void OnAssign(PlayerControl pc)
        {
        }

        public void OnProtect(PlayerControl pc, PlayerControl target)
        {
            if (!pc.RpcCheckAndMurder(target, check: true)) return;
            ScheduledDeaths.TryAdd(target.PlayerId, Utils.TimeStamp);
        }

        public static void Update(PlayerControl pc)
        {
            foreach (var death in ScheduledDeaths)
            {
                var player = Utils.GetPlayerById(death.Key);
                if (player == null || !player.IsAlive()) continue;

                if (Utils.TimeStamp - death.Value < Duration.GetInt())
                {
                    Utils.NotifyRoles(SpecifySeer: player, SpecifyTarget: player);
                    return;
                }

                if (pc.RpcCheckAndMurder(player, check: true)) player.Suicide(realKiller: pc);
            }
        }

        public static void OnMeetingStart()
        {
            foreach (var id in ScheduledDeaths.Keys)
            {
                var pc = Utils.GetPlayerById(id);
                if (pc == null || !pc.IsAlive()) continue;

                pc.Suicide();
            }

            ScheduledDeaths.Clear();
        }

        public static string GetSuffix(PlayerControl seer)
        {
            if (!ScheduledDeaths.TryGetValue(seer.PlayerId, out var ts)) return string.Empty;

            var timeLeft = Duration.GetInt() - (Utils.TimeStamp - ts) + 1;
            var colors = GetColors();
            return string.Format(Translator.GetString("Bloodmoon.Suffix"), timeLeft, colors.TextColor, colors.TimeColor);

            (string TextColor, string TimeColor) GetColors() => timeLeft switch
            {
                > 5 => ("#ffff00", "#ffa500"),
                _ => ("#ff0000", "#ffff00")
            };
        }
    }
}