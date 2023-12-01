using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static TOHE.Options;

namespace TOHE.Roles.Crewmate;

public static class Marshall
{
    private static readonly int Id = 9400;
    private static readonly List<byte> playerIdList = [];
    private static Color RoleColor = Utils.GetRoleColor(CustomRoles.Marshall);

    public static OptionItem OptionMadmateCanFindMarshall;

    public static bool MadmateCanFindMarshall;

    private static readonly Dictionary<byte, bool> IsExposed = [];
    private static readonly Dictionary<byte, bool> IsComplete = [];

    private static readonly HashSet<byte> TargetList = [];
    private static readonly Dictionary<byte, Color> TargetColorlist = [];

    public static void SetupCustomOption()
    {
        SetupRoleOptions(Id, TabGroup.CrewmateRoles, CustomRoles.Marshall);
        //    OptionMadmateCanFindMarshall = BooleanOptionItem.Create(Id + 14, "MadmateCanFindMarshall", false, TabGroup.CrewmateRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Marshall]);
        OverrideTasksData.Create(Id + 20, TabGroup.CrewmateRoles, CustomRoles.Marshall);
    }
    public static void Init()
    {
        playerIdList.Clear();
        IsEnable = false;

        //MadmateCanFindMarshall = OptionMadmateCanFindMarshall.GetBool();

        IsExposed.Clear();
        IsComplete.Clear();

        TargetList.Clear();
        TargetColorlist.Clear();
    }

    public static void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        IsEnable = true;

        IsExposed[playerId] = false;
        IsComplete[playerId] = false;
    }

    public static bool IsEnable;
    public static bool IsThisRole(byte playerId) => playerIdList.Contains(playerId);
    private static bool GetExpose(PlayerControl pc)
    {
        if (!IsThisRole(pc.PlayerId) || !pc.IsAlive() || pc.Is(CustomRoles.Madmate)) return false;

        var marshallId = pc.PlayerId;
        return IsExposed[marshallId];
    }
    private static bool IsMarshallTarget(PlayerControl target) => IsEnable && (target.Is(CustomRoleTypes.Crewmate) || (target.Is(CustomRoles.Madmate) && MadmateCanFindMarshall));
    public static void CheckTask(PlayerControl marshall)
    {
        if (!marshall.IsAlive() || marshall.Is(CustomRoles.Madmate)) return;

        var marshallId = marshall.PlayerId;

        if (!IsExposed[marshallId]) IsExposed[marshallId] = true;

        NameNotifyManager.Notify(marshall, Translator.GetString("MarshallDoneTasks"));
        IsComplete[marshallId] = true;
    }

    /// <summary>
    /// タスクが進んだスニッチに警告マーク
    /// </summary>
    /// <param name="seer">キラーの場合有効</param>
    /// <param name="target">スニッチの場合有効</param>
    /// <returns></returns>
    public static string GetWarningMark(PlayerControl seer, PlayerControl target)
        => IsMarshallTarget(seer) && GetExpose(target) ? Utils.ColorString(RoleColor, "★") : string.Empty;

    /// <summary>
    /// キラーからスニッチに対する矢印
    /// </summary>
    /// <param name="seer">キラーの場合有効</param>
    /// <param name="target">キラーの場合有効</param>
    /// <returns></returns>
    public static string GetWarningArrow(PlayerControl seer, PlayerControl target = null)
    {
        if (GameStates.IsMeeting || !IsMarshallTarget(seer)) return string.Empty;
        if (target != null && seer.PlayerId != target.PlayerId) return string.Empty;

        if (!playerIdList.Any(s => !Main.PlayerStates[s].IsDead && IsExposed[s])) return string.Empty;

        var warning = "★";

        return Utils.ColorString(RoleColor, warning);
    }
    /// <summary>
    /// スニッチからキラーへの矢印
    /// </summary>
    /// <param name="seer">スニッチの場合有効</param>
    /// <param name="target">スニッチの場合有効</param>
    /// <returns></returns>

    public static void OnCompleteTask(PlayerControl player)
    {
        if (!IsThisRole(player.PlayerId) /*|| player.Is(CustomRoles.Madmate)*/) return;
        CheckTask(player);
    }
}
